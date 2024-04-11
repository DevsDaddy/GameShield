using System;
using UnityEngine;

namespace DevsDaddy.GameShield.Demo.UI
{
    /// <summary>
    /// Base View
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseView : MonoBehaviour
    {
        // View Components
        private Canvas canvas;
        private CanvasGroup group;
        
        /// <summary>
        /// On Awake
        /// </summary>
        private void Awake() {
            canvas = GetComponent<Canvas>();
            group = GetComponent<CanvasGroup>();
            Hide();
            OnInitialized();
        }
        public virtual void OnInitialized(){}

        /// <summary>
        /// Show View
        /// </summary>
        public void Show() {
            Toggle(true);
        }

        /// <summary>
        /// Hide View
        /// </summary>
        public void Hide() {
            Toggle(false);
        }

        /// <summary>
        /// Toggle View
        /// </summary>
        /// <param name="isEnabled"></param>
        public void Toggle(bool isEnabled) {
            group.alpha = isEnabled ? 1f : 0f;
            group.interactable = isEnabled;
            group.blocksRaycasts = isEnabled;
            canvas.enabled = isEnabled;
        }
    }
}