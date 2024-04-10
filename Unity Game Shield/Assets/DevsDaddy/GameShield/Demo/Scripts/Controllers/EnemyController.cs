using System;
using System.Collections.Generic;
using DevsDaddy.GameShield.Core.Modules.Teleport;
using DevsDaddy.GameShield.Core.Payloads;
using DevsDaddy.GameShield.Demo.Enums;
using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;
using UnityEngine.AI;

namespace DevsDaddy.GameShield.Demo.Controllers
{
    /// <summary>
    /// Basic Enemy Controller
    /// </summary>
    internal class EnemyController : MonoBehaviour
    {
        [Header("Initial Position")] 
        [SerializeField] private Vector3 initialPosition;
        [SerializeField] private Quaternion initialRotation;

        [Header("Enemy AI")] 
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private List<Transform> waypoints;

        private int currentWaypoint = 0;
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
                    MaxSpeed = agent.speed,
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
        /// On Update
        /// </summary>
        private void Update() {
            DetectNewWaypoint();
        }

        /// <summary>
        /// Detect New Waypoint
        /// </summary>
        private void DetectNewWaypoint() {
            if (Vector3.Distance(transform.position, waypoints[currentWaypoint].position) < 2f) {
                int newWaypoint = (currentWaypoint + 1 < waypoints.Count) ? currentWaypoint + 1 : 0;
                NavigateToWaypoint(newWaypoint);
            }
        }

        /// <summary>
        /// Navigate to Waypoint
        /// </summary>
        /// <param name="waypoint"></param>
        private void NavigateToWaypoint(int waypoint) {
            currentWaypoint = waypoint;
            agent.SetDestination(waypoints[currentWaypoint].position);
        }

        /// <summary>
        /// Reset Enemy
        /// </summary>
        private void ResetEnemey() {
            agent.enabled = false;
            transform.SetPositionAndRotation(initialPosition, initialRotation);
            agent.enabled = true;
            NavigateToWaypoint(0);
        }

        /// <summary>
        /// Bind Events
        /// </summary>
        private void BindEvents() {
            EventMessenger.Main.Subscribe<OnEnemyStateChanged>(OnStateChanged);
            EventMessenger.Main.Subscribe<SecurityModuleInitialized>(TeleportDetectorInitialized);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents() {
            EventMessenger.Main.Unsubscribe<OnEnemyStateChanged>(OnStateChanged);
            EventMessenger.Main.Unsubscribe<SecurityModuleInitialized>(TeleportDetectorInitialized);
        }

        /// <summary>
        /// On Game State Changed
        /// </summary>
        /// <param name="payload"></param>
        private void OnStateChanged(OnEnemyStateChanged payload) {
            if (payload.State == BaseState.Start || payload.State == BaseState.Restart) {
                ResetEnemey();
            }
        }
    }
}