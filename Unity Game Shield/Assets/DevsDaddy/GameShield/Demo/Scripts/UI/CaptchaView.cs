using System;
using System.Collections;
using System.Collections.Generic;
using DevsDaddy.GameShield.Core.Constants;
using DevsDaddy.GameShield.Core.Modules.Captcha;
using DevsDaddy.GameShield.Demo.UI.Elements;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace DevsDaddy.GameShield.Demo.UI
{
    /// <summary>
    /// Rewarded Captcha View
    /// </summary>
    internal class CaptchaView : BaseView
    {
        [Header("Captcha UI Setup")] 
        [SerializeField] private Button applyButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private List<Sprite> availableIcons;

        [Header("Captcha Containers")]
        [SerializeField] private RectTransform baseLayout;
        [SerializeField] private RectTransform solveLayout;
        [SerializeField] private GameObject baseImageTemplate;
        [SerializeField] private GameObject solveButtonTemplate;
        
        // Current Data
        private List<int> currentClicksMap = new List<int>();
        private RewardedCaptchaData currentData;
        
        // Current Filled
        private List<GameObject> currentImages = new List<GameObject>();
        private List<GameObject> currentButtons = new List<GameObject>();

        /// <summary>
        /// On View Initialized
        /// </summary>
        public override void OnInitialized() {
            BindEvents();
        }

        /// <summary>
        /// On View Destroyed
        /// </summary>
        private void OnDestroy() {
            UnbindEvents();
        }

        /// <summary>
        /// Bind Events
        /// </summary>
        private void BindEvents() {
            EventMessenger.Main.Subscribe<RequestCaptchaPayload>(OnCaptchaRequested);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents() {
            EventMessenger.Main.Unsubscribe<RequestCaptchaPayload>(OnCaptchaRequested);
        }

        /// <summary>
        /// On Captcha Requested
        /// </summary>
        /// <param name="payload"></param>
        private void OnCaptchaRequested(RequestCaptchaPayload payload) {
            // Working Module
            RewardedCaptcha workingModule = Core.GameShield.Main.GetModule<RewardedCaptcha>();
            if (workingModule == null) {
                Debug.LogError($"{GeneralStrings.LOG_PREFIX} Failed to initialize captcha. No RewardedCaptcha Module connected to GameShield.");
                return;
            }

            // Generate Captcha Data
            currentData = workingModule.GenerateCaptchaData(payload, availableIcons.Count);
            
            // Check Available Icons
            string error = "";
            if (availableIcons == null || availableIcons.Count < 1) {
                error = "Failed to show captcha. No available icons in view presented";
                Debug.LogError($"{GeneralStrings.LOG_PREFIX} {error}");
                payload.OnError?.Invoke(error);
                return;
            }
            
            // Fill Captcha Areas
            if (payload.NumOfImages < 1) {
                error = "Failed to show captcha. Requested less than 1 images counter.";
                Debug.LogError($"{GeneralStrings.LOG_PREFIX} {error}");
                payload.OnError?.Invoke(error);
                return;
            }
            
            // Buttons Handlers
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(()=> ProcessCaptchaAnswer(payload, workingModule));
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => {
                payload.OnCanceled?.Invoke();
                Hide();
            });
            resetButton.onClick.RemoveAllListeners();
            resetButton.onClick.AddListener(() => {
                OnCaptchaRequested(payload);
            });

            RefillNeedAnswer();
            StartCoroutine(RefillAnswerFiled());
            Show();
        }

        /// <summary>
        /// Refill Required Answer Hint
        /// </summary>
        private void RefillNeedAnswer() {
            // Clear Images
            currentImages.ForEach(image => Destroy(image));
            currentImages.Clear();
            
            for (int i = 0; i < currentData.RequiredOrder.Length; i++) {
                GameObject newItem = GameObject.Instantiate(baseImageTemplate, baseLayout.transform);
                newItem.GetComponent<Image>().sprite = availableIcons[currentData.RequiredOrder[i]];
                currentImages.Add(newItem);
            }
        }

        /// <summary>
        /// Refill Answer Field
        /// </summary>
        private IEnumerator RefillAnswerFiled() {
            // Clear Buttons
            yield return new WaitForEndOfFrame();
            currentClicksMap.Clear();
            currentButtons.ForEach(btnObj => Destroy(btnObj));
            currentButtons.Clear();
            
            applyButton.interactable = false;
            
            for (int i = 0; i < currentData.CurrentFieldOrder.Length; i++) {
                int index = i;
                GameObject newItem = GameObject.Instantiate(solveButtonTemplate, solveLayout.transform);
                RectTransform newItemRect = newItem.GetComponent<RectTransform>();
                newItemRect.SetParent(solveLayout);
                newItemRect.localPosition =
                    GetRandomPosition(newItemRect.rect, index, currentData.CurrentFieldOrder.Length);
                newItemRect.localRotation = GetRandomRotation();
                newItem.GetComponent<Image>().sprite = availableIcons[currentData.CurrentFieldOrder[index]];
                CaptchaButton button = newItem.GetComponent<CaptchaButton>();
                button.OnPressed = () => {
                    currentClicksMap.Add(currentData.CurrentFieldOrder[index]);
                    button.SetCounter(currentClicksMap.Count);
                    CheckApplyAvailable();
                };
                currentButtons.Add(newItem);
            }
        }

        /// <summary>
        /// Get Random position for Item
        /// </summary>
        /// <param name="itemRect"></param>
        /// <param name="chunk"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private Vector3 GetRandomPosition(Rect itemRect, int chunk, int max) {
            // Base Data
            chunk += 1;
            float width = solveLayout.rect.width;
            float height = solveLayout.rect.height;
            float maxXCell = width / max;
            
            // Limit Data for Chunk
            float minX = (maxXCell * chunk) - maxXCell;
            float maxX = (maxXCell * chunk) - itemRect.width;
            
            float minY = -1 * itemRect.height;
            float maxY = (-1 * height) + itemRect.height;

            Vector3 position = Vector3.zero;
            position.x = Random.Range(minX, maxX);
            position.y = Random.Range(minY, maxY);
            return position;
        }

        /// <summary>
        /// Get Random Rotation
        /// </summary>
        /// <returns></returns>
        private Quaternion GetRandomRotation() {
            Quaternion newRot = Quaternion.Euler(0,0,Random.Range(0f, 90f));
            return newRot;
        }

        /// <summary>
        /// Check Apply Available
        /// </summary>
        private void CheckApplyAvailable() {
            applyButton.interactable = (currentClicksMap.Count == currentData.RequiredOrder.Length);
        }

        /// <summary>
        /// Process Captcha Answer
        /// </summary>
        private void ProcessCaptchaAnswer(RequestCaptchaPayload payload, RewardedCaptcha worker) {
            bool isRight = worker.IsRightOrder(currentClicksMap);
            if (!isRight) {
                payload.OnError?.Invoke("You clicked on the images in the wrong sequence. Try resetting the image and starting over.");
                return;
            }
            
            payload.OnComplete?.Invoke();
            Hide();
        }
    }
}