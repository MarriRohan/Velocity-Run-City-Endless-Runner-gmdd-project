using UnityEngine;

namespace NeonVelocity
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Target & Offsets")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 3.2f, -6.5f);
        [SerializeField] private float smoothSpeedX = 10.0f;
        [SerializeField] private float smoothSpeedY = 12.0f;
        [SerializeField] private float smoothSpeedZ = 20.0f;
        [SerializeField] private float lookAheadDistance = 14.0f;

        [Header("Dynamic FOV")]
        [SerializeField] private Camera cam;
        [SerializeField] private float baseFOV = 62.0f;
        [SerializeField] private float maxFOV = 78.0f;
        [SerializeField] private float fovSmoothSpeed = 4.0f;

        private float currentShakeIntensity = 0f;
        private float shakeDecay = 4.0f;
        private bool isCrashCam = false;
        private Vector3 crashCamPosition;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (cam == null) cam = GetComponent<Camera>();
            if (cam != null) cam.fieldOfView = baseFOV;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            isCrashCam = false;
        }

        public void Shake(float intensity)
        {
            currentShakeIntensity = Mathf.Max(currentShakeIntensity, intensity);
        }

        public void TriggerCrashCam()
        {
            isCrashCam = true;
            if (target != null)
            {
                crashCamPosition = target.position + new Vector3(0f, 4.5f, -8.0f);
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            if (isCrashCam)
            {
                // Cinematic slow pull-back to frame crash
                transform.position = Vector3.Lerp(transform.position, crashCamPosition, Time.deltaTime * 3.0f);
                Vector3 lookPoint = target.position + Vector3.up * 1.0f;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPoint - transform.position), Time.deltaTime * 4.0f);
                return;
            }

            // Normal smooth following
            Vector3 desiredPos = target.position + followOffset;

            // Dampen X independently so lane jumps don't snap the camera
            float newX = Mathf.Lerp(transform.position.x, desiredPos.x, smoothSpeedX * Time.deltaTime);
            float newY = Mathf.Lerp(transform.position.y, desiredPos.y, smoothSpeedY * Time.deltaTime);
            float newZ = Mathf.Lerp(transform.position.z, desiredPos.z, smoothSpeedZ * Time.deltaTime);

            Vector3 finalPos = new Vector3(newX, newY, newZ);

            // Screen Shake
            if (currentShakeIntensity > 0.001f)
            {
                Vector3 shakeOffset = Random.insideUnitSphere * currentShakeIntensity;
                finalPos += shakeOffset;
                currentShakeIntensity = Mathf.MoveTowards(currentShakeIntensity, 0f, Time.deltaTime * shakeDecay);
            }

            transform.position = finalPos;

            // Look ahead along highway
            Vector3 lookTarget = target.position + Vector3.forward * lookAheadDistance + Vector3.up * 1.2f;
            transform.rotation = Quaternion.LookRotation(lookTarget - transform.position);

            // Dynamic FOV
            if (cam != null && PlayerController.Instance != null)
            {
                float speedRatio = Mathf.InverseLerp(PlayerController.Instance.MinSpeed, PlayerController.Instance.MaxSpeed, PlayerController.Instance.CurrentSpeed);
                float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedRatio);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);
            }
        }
    }
}
