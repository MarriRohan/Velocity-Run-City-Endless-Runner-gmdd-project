using UnityEngine;

namespace NeonVelocity
{
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("Model Root & Parts")]
        [SerializeField] private Transform modelRoot;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform torso;
        [SerializeField] private Transform backpackCore;

        [Header("Animation Speeds")]
        [SerializeField] private float runCadenceMultiplier = 0.65f;
        [SerializeField] private float armSwingAngle = 38.0f;
        [SerializeField] private float legStrideAngle = 45.0f;
        [SerializeField] private float bodyBounceHeight = 0.08f;

        private float runPhase = 0f;
        private float stepTimer = 0f;
        private Vector3 initialModelLocalPos;
        private Quaternion initialModelLocalRot;

        private void Start()
        {
            if (modelRoot != null)
            {
                initialModelLocalPos = modelRoot.localPosition;
                initialModelLocalRot = modelRoot.localRotation;
            }
        }

        public void Animate(PlayerState state, float speed, float laneLeanAngle)
        {
            if (modelRoot == null) return;

            switch (state)
            {
                case PlayerState.Running:
                    UpdateRunAnimation(speed, laneLeanAngle);
                    break;
                case PlayerState.Jumping:
                    UpdateJumpAnimation(laneLeanAngle);
                    break;
                case PlayerState.Sliding:
                    UpdateSlideAnimation(laneLeanAngle);
                    break;
                case PlayerState.Crashed:
                    UpdateCrashAnimation();
                    break;
            }
        }

        private void UpdateRunAnimation(float speed, float laneLeanAngle)
        {
            float cadence = speed * runCadenceMultiplier;
            runPhase += Time.deltaTime * cadence;

            float legAngle = Mathf.Sin(runPhase) * legStrideAngle;
            float armAngle = Mathf.Sin(runPhase + Mathf.PI) * armSwingAngle;
            float bounce = Mathf.Abs(Mathf.Sin(runPhase * 2f)) * bodyBounceHeight;

            // Model Root bounce and lane tilt
            modelRoot.localPosition = initialModelLocalPos + new Vector3(0f, bounce, 0f);
            modelRoot.localRotation = Quaternion.Euler(0f, 0f, laneLeanAngle);

            // Limb rotations
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(legAngle, 0f, 0f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-legAngle, 0f, 0f);
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(armAngle, 0f, 0f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(-armAngle, 0f, 0f);

            // Audio Footstep cadence
            stepTimer += Time.deltaTime * cadence;
            if (stepTimer >= Mathf.PI)
            {
                stepTimer = 0f;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayFootstep();
                }
            }
        }

        private void UpdateJumpAnimation(float laneLeanAngle)
        {
            // Tucked legs, arms angled back, slight upward tilt
            modelRoot.localPosition = initialModelLocalPos + new Vector3(0f, 0.1f, 0f);
            modelRoot.localRotation = Quaternion.Euler(-12f, 0f, laneLeanAngle);

            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(30f, 0f, -5f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(35f, 0f, 5f);
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(-45f, 0f, 15f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(-45f, 0f, -15f);
        }

        private void UpdateSlideAnimation(float laneLeanAngle)
        {
            // Low kneeling crouch, body pushed forward
            modelRoot.localPosition = initialModelLocalPos + new Vector3(0f, -0.45f, 0.15f);
            modelRoot.localRotation = Quaternion.Euler(25f, 0f, laneLeanAngle);

            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(75f, 0f, 0f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(-20f, 0f, 0f);
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(60f, 0f, 25f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(60f, 0f, -25f);
        }

        private void UpdateCrashAnimation()
        {
            // Stumble backward ragdoll rotation
            modelRoot.localPosition = initialModelLocalPos + new Vector3(0f, -0.2f, -0.4f);
            modelRoot.localRotation = Quaternion.Euler(-45f, 25f, -30f);

            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(-30f, 15f, 0f);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(40f, -15f, 0f);
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(-60f, 0f, 30f);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(-70f, 0f, -30f);
        }

        public void ResetAnimation()
        {
            runPhase = 0f;
            stepTimer = 0f;
            if (modelRoot != null)
            {
                modelRoot.localPosition = initialModelLocalPos;
                modelRoot.localRotation = initialModelLocalRot;
            }
        }
    }
}
