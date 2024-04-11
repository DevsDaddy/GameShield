using System;
using System.Collections;
using System.Collections.Generic;
using DevsDaddy.GameShield.Core.Modules.Teleport;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.GameShield.Demo.Enums;
using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Demo.Controllers
{
    /// <summary>
    /// Basic Player Controller
    /// </summary>
    internal class PlayerController : MonoBehaviour
    {
        [Header("Initial Position")] 
        [SerializeField] private Vector3 initialPosition;
        [SerializeField] private Quaternion initialRotation;
        
        [Header("Player Controller Setup")]
        [SerializeField] private Rigidbody rigidbody;

        // Movement parameters
        private float moveSpeed = 20;
        private float rotationSpeed = 4;
        private float runningSpeed;
        private float vaxis, haxis;
        private Vector3 movement;
        
        // Player Controls States
        private bool IsControlsLocked = false;
        private TeleportDetector currentDetector;
        private TeleportTargetChecker currentChecker;

        /// <summary>
        /// On Awake
        /// </summary>
        private void Awake() {
            BindEvents();
        }

        /// <summary>
        /// Teleport Detector Initialized
        /// </summary>
        private void TeleportDetectorInitialized(SecurityModuleInitialized payload) {
            if (payload.Module.GetType() == typeof(TeleportDetector)) {
                currentDetector = (TeleportDetector)payload.Module;
                currentChecker = new TeleportTargetChecker {
                    LastPosition = transform.position,
                    MaxSpeed = moveSpeed,
                    Target = transform
                };
                currentDetector.AddTarget(currentChecker);
            }
        }

        /// <summary>
        /// On Destroy
        /// </summary>
        private void OnDestroy() {
            if(currentChecker != null)
                currentDetector.RemoveTarget(currentChecker);
            UnbindEvents();
        }

        /// <summary>
        /// Update
        /// </summary>
        private void FixedUpdate() {
            // Fully Locked Controls
            if(IsControlsLocked) return;
            MoveCharacter();
        }

        /// <summary>
        /// Move Character
        /// </summary>
        private void MoveCharacter() {
            /*  Controller Mappings */
            vaxis = Input.GetAxis("Vertical");
            haxis = Input.GetAxis("Horizontal");

            //Simplified...
            runningSpeed = vaxis;
            movement = new Vector3(0, 0f, runningSpeed * 8);
            movement = transform.TransformDirection(movement);
            rigidbody.AddForce(movement * moveSpeed);

            if ((Input.GetAxis("Vertical") != 0f || Input.GetAxis("Horizontal") != 0f))
            {
                if (Input.GetAxis("Vertical") >= 0)
                    transform.Rotate(new Vector3(0, haxis * rotationSpeed, 0));
                else
                    transform.Rotate(new Vector3(0, -haxis * rotationSpeed, 0));

            }
        }

        /// <summary>
        /// Reset Player
        /// </summary>
        private void ResetPlayer() {
            transform.SetPositionAndRotation(initialPosition, initialRotation);
            if (currentDetector != null && currentChecker != null) {
                currentDetector.RemoveTarget(currentChecker);
                currentDetector.AddTarget(currentChecker);
            }
        }

        /// <summary>
        /// Bind Events
        /// </summary>
        private void BindEvents() {
            EventMessenger.Main.Subscribe<OnPlayerStateChanged>(OnStateChanged);
            EventMessenger.Main.Subscribe<SecurityModuleInitialized>(TeleportDetectorInitialized);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents() {
            EventMessenger.Main.Unsubscribe<OnPlayerStateChanged>(OnStateChanged);
            EventMessenger.Main.Unsubscribe<SecurityModuleInitialized>(TeleportDetectorInitialized);
        }

        /// <summary>
        /// On Game State Changed
        /// </summary>
        /// <param name="payload"></param>
        private void OnStateChanged(OnPlayerStateChanged payload) {
            IsControlsLocked = payload.State == BaseState.Stop;
            if (payload.State == BaseState.Start || payload.State == BaseState.Restart) {
                ResetPlayer();
            }
        }
    }
}
