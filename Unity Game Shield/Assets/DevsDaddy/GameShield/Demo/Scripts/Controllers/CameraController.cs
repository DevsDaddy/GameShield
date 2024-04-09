using System;
using DevsDaddy.GameShield.Demo.Enums;
using DevsDaddy.GameShield.Demo.Payloads;
using DevsDaddy.Shared.EventFramework;
using UnityEngine;

namespace DevsDaddy.GameShield.Demo.Controllers
{
    /// <summary>
    /// Basic Camera Controller
    /// </summary>
    internal class CameraController : MonoBehaviour
    {
        [Header("Initial Position")] 
        [SerializeField] private Vector3 initialPosition;
        [SerializeField] private Quaternion initialRotation;
        
        [Header("Camera Setup")]
        [Space(10.0f)]
        [Tooltip("Point aimed by the camera")]
        public Transform Target;

        [Tooltip("Maximum distance between the camera and Target")]
        public float Distance = 2;

        [Tooltip("Distance lerp factor")]
        [Range(.0f, 1.0f)]
        public float LerpSpeed = .1f;

        [Space(10.0f)]
        [Tooltip("Collision parameters")]
        public TraceInfo RayTrace = new TraceInfo { Thickness = .2f };

        [Tooltip("Camera pitch limitations")]
        public LimitsInfo PitchLimits = new LimitsInfo { Minimum = -60.0f, Maximum = 60.0f };

        [Tooltip("Input axes used to control the camera")]
        public InputInfo InputAxes = new InputInfo
        {
            Horizontal = new InputAxisInfo { Name = "Mouse X", Sensitivity = 15.0f },
            Vertical = new InputAxisInfo { Name = "Mouse Y", Sensitivity = 15.0f }
        };

        // Gameplay State
        private bool InGameplay = false;
        private float _pitch;
        private float _distance;

        /// <summary>
        /// On Awake
        /// </summary>
        private void Awake() {
            BindEvents();
        }

        /// <summary>
        /// On Start
        /// </summary>
        private void Start() {
            SetInitialPosition();
        }

        /// <summary>
        /// On Destroy
        /// </summary>
        private void OnDestroy() {
            UnbindEvents();
        }

        /// <summary>
        /// Set Camera Initial Position and Rotation
        /// </summary>
        private void SetInitialPosition() {
            transform.SetPositionAndRotation(initialPosition, initialRotation);
        }

        /// <summary>
        /// Set Gameplay Initial Position
        /// </summary>
        private void SetGameplayInitialPosition() {
            _pitch = Mathf.DeltaAngle(0, -transform.localEulerAngles.x);
            _distance = Distance;
        }

        /// <summary>
        /// On Update
        /// </summary>
        private void Update() {
            if(!InGameplay) return;
            
            float yaw = transform.localEulerAngles.y + Input.GetAxis(InputAxes.Horizontal.Name) * InputAxes.Horizontal.Sensitivity;
            _pitch += Input.GetAxis(InputAxes.Vertical.Name) * InputAxes.Vertical.Sensitivity;
            _pitch = Mathf.Clamp(_pitch, PitchLimits.Minimum, PitchLimits.Maximum);
            transform.localEulerAngles = new Vector3(-_pitch, yaw, 0);
        }
        
        /// <summary>
        /// On Late Update
        /// </summary>
        private void LateUpdate()
        {
            if (Target == null) return;

            var startPos = Target.position;
            var endPos = startPos - transform.forward * Distance;
            var result = Vector3.zero;

            RayCast(startPos, endPos, ref result, RayTrace.Thickness);
            var resultDistance = Vector3.Distance(Target.position, result);
        
            if (resultDistance <= _distance)    // closest collision
            {
                transform.position = result;
                _distance = resultDistance;
            }
            else
            {
                _distance = Mathf.Lerp(_distance, resultDistance, LerpSpeed);
                transform.position = startPos - transform.forward * _distance;
            }
        }
        
        /// <summary>
        /// Camera Raycast
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="result"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        private bool RayCast(Vector3 start, Vector3 end, ref Vector3 result, float thickness)
        {
            var direction = end - start;
            var distance = Vector3.Distance(start, end);

            RaycastHit hit;
            if (Physics.SphereCast(new Ray(start, direction), thickness, out hit, distance, RayTrace.CollisionMask.value))
            {
                result = hit.point + hit.normal * RayTrace.Thickness;
                return true;
            }
            else
            {
                result = end;
                return false;
            }
        }

        /// <summary>
        /// Bind Events
        /// </summary>
        private void BindEvents() {
            EventMessenger.Main.Subscribe<OnCameraStateChanged>(OnStateChanged);
        }

        /// <summary>
        /// Unbind Events
        /// </summary>
        private void UnbindEvents() {
            EventMessenger.Main.Unsubscribe<OnCameraStateChanged>(OnStateChanged);
        }

        /// <summary>
        /// On State Changed
        /// </summary>
        /// <param name="payload"></param>
        private void OnStateChanged(OnCameraStateChanged payload) {
            InGameplay = payload.State != BaseState.Stop;
            if (payload.State == BaseState.Stop)
                SetInitialPosition();
            else
                SetGameplayInitialPosition();
        }
        
        [System.Serializable]
        public struct LimitsInfo
        {
            [Tooltip("Minimum pitch angle, in the range [-90, Maximum]")]
            public float Minimum;

            [Tooltip("Maximum pitch angle, in the range [Minimum, 90]")]
            public float Maximum;
        }
        
        [System.Serializable]
        public struct TraceInfo
        {
            [Tooltip("Ray thickness")]
            public float Thickness;

            [Tooltip("Layers the camera collide with")]
            public LayerMask CollisionMask;
        }

        [System.Serializable]
        public struct InputInfo
        {
            [Tooltip("Horizontal axis")]
            public InputAxisInfo Horizontal;

            [Tooltip("Vertical axis")]
            public InputAxisInfo Vertical;
        }

        [System.Serializable]
        public struct InputAxisInfo
        {
            [Tooltip("Input axis name")]
            public string Name;

            [Tooltip("Axis sensitivity")]
            public float Sensitivity;
        }
    }
}