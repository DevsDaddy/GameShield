using System;
using UnityEngine;
using UnityEngine.UI;

namespace DevsDaddy.GameShield.Demo.UI.Elements
{
    [RequireComponent(typeof(Button))]
    [DisallowMultipleComponent]
    public class CaptchaButton : MonoBehaviour
    {
        [Header("References Setup")] 
        [SerializeField] private GameObject counter;

        private Button m_Button;

        // On Pressed
        public Action OnPressed;
        public bool IsPressed = false;

        /// <summary>
        /// On Awake
        /// </summary>
        private void Awake() {
            m_Button = GetComponent<Button>();
            counter.SetActive(false);
        }

        private void Start() {
            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(() => {
                m_Button.interactable = false;
                IsPressed = true;
                OnPressed?.Invoke();
            });
        }

        /// <summary>
        /// Set Counter
        /// </summary>
        /// <param name="count"></param>
        public void SetCounter(int count) {
            counter.SetActive(true);
            counter.GetComponentInChildren<Text>().text = count.ToString("N0");
        }
    }
}