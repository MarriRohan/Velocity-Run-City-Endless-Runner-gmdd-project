using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NeonVelocity
{
    public enum PlayerState
    {
        Running,
        Jumping,
        Sliding,
        Crashed
    }

    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Lane Settings")]
        [SerializeField] private float laneDistance = 3.5f;
        [SerializeField] private float laneChangeSpeed = 16.0f;
        [SerializeField] private float maxLeanAngle = 18.0f;

        [Header("Movement Speeds")]
        [SerializeField] private float minSpeed = 18.0f;     // ~65 km/h
        [SerializeField] private float maxSpeed = 35.0f;     // ~126 km/h
        [SerializeField] private float acceleration = 0.25f; // Speed increase per second
        [SerializeField] private float currentSpeed;

        [Header("Jump & Slide")]
        [SerializeField] private float jumpVelocity = 12.0f;
        [SerializeField] private float gravity = 36.0f;
        [SerializeField] private float slideDuration = 0.75f;

        [Header("Colliders")]
        [SerializeField] private CapsuleCollider playerCollider;
        [SerializeField] private float normalHeight = 1.8f;
        [SerializeField] private Vector3 normalCenter = new Vector3(0f, 0.9f, 0f);
        [SerializeField] private float slideHeight = 0.7f;
        [SerializeField] private Vector3 slideCenter = new Vector3(0f, 0.35f, 0f);

        [Header("Magnetic Pull")]
        [SerializeField] private float magnetRadius = 3.5f;

        [Header("Components")]
        [SerializeField] private PlayerAnimator playerAnimator;

        private int targetLane = 0; // -1: Left, 0: Center, 1: Right
        private float currentX = 0f;
        private float verticalVelocity = 0f;
        private bool isGrounded = true;
        private float slideTimer = 0f;
        private PlayerState currentState = PlayerState.Running;
        private bool isDead = false;

        // Touch Input
        private Vector2 touchStartPos;
        private bool isTouching = false;
        private const float MIN_SWIPE_DISTANCE = 35f;

        public float CurrentSpeed => currentSpeed;
        public float SpeedKmh => currentSpeed * 3.6f;
        public float MinSpeed => minSpeed;
        public float MaxSpeed => maxSpeed;
        public bool IsDead => isDead;
        public PlayerState State => currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            currentSpeed = minSpeed;
            if (playerCollider == null) playerCollider = GetComponent<CapsuleCollider>();
            if (playerAnimator == null) playerAnimator = GetComponent<PlayerAnimator>();
        }

        private void Start()
        {
            ResetPlayer();
        }

        public void ResetPlayer()
        {
            targetLane = 0;
            currentX = 0f;
            verticalVelocity = 0f;
            isGrounded = true;
            slideTimer = 0f;
            isDead = false;
            currentSpeed = minSpeed;
            currentState = PlayerState.Running;

            transform.position = new Vector3(0f, 0f, 0f);
            SetColliderNormal();

            playerAnimator?.ResetAnimation();
        }

        private void Update()
        {
            if (isDead || GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            {
                return;
            }

            HandleInput();
            UpdateSpeed();
            UpdateMovement();
            UpdateMagnet();
            UpdateAnimation();
        }

        private void HandleInput()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)
                {
                    MoveLane(-1);
                }
                else if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame)
                {
                    MoveLane(1);
                }

                if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                {
                    Jump();
                }
                else if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame || kb.leftCtrlKey.wasPressedThisFrame)
                {
                    Slide();
                }
            }

            var pad = Gamepad.current;
            if (pad != null)
            {
                if (pad.dpad.left.wasPressedThisFrame || pad.leftStick.left.wasPressedThisFrame)
                {
                    MoveLane(-1);
                }
                else if (pad.dpad.right.wasPressedThisFrame || pad.leftStick.right.wasPressedThisFrame)
                {
                    MoveLane(1);
                }

                if (pad.buttonSouth.wasPressedThisFrame || pad.dpad.up.wasPressedThisFrame)
                {
                    Jump();
                }
                else if (pad.buttonEast.wasPressedThisFrame || pad.dpad.down.wasPressedThisFrame)
                {
                    Slide();
                }
            }

            var ts = Touchscreen.current;
            if (ts != null)
            {
                if (ts.primaryTouch.press.wasPressedThisFrame)
                {
                    touchStartPos = ts.primaryTouch.position.ReadValue();
                    isTouching = true;
                }
                else if (isTouching && !ts.primaryTouch.press.isPressed)
                {
                    isTouching = false;
                    Vector2 delta = ts.primaryTouch.position.ReadValue() - touchStartPos;
                    if (delta.magnitude >= MIN_SWIPE_DISTANCE)
                    {
                        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        {
                            if (delta.x > 0) MoveLane(1);
                            else MoveLane(-1);
                        }
                        else
                        {
                            if (delta.y > 0) Jump();
                            else Slide();
                        }
                    }
                }
            }
#else
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveLane(-1);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveLane(1);
            }

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftControl))
            {
                Slide();
            }

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == UnityEngine.TouchPhase.Began)
                {
                    touchStartPos = touch.position;
                    isTouching = true;
                }
                else if (touch.phase == UnityEngine.TouchPhase.Ended && isTouching)
                {
                    isTouching = false;
                    Vector2 delta = touch.position - touchStartPos;
                    if (delta.magnitude >= MIN_SWIPE_DISTANCE)
                    {
                        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        {
                            if (delta.x > 0) MoveLane(1);
                            else MoveLane(-1);
                        }
                        else
                        {
                            if (delta.y > 0) Jump();
                            else Slide();
                        }
                    }
                }
            }
#endif
        }

        private void MoveLane(int direction)
        {
            int newLane = Mathf.Clamp(targetLane + direction, -1, 1);
            if (newLane != targetLane)
            {
                targetLane = newLane;
            }
        }

        private void Jump()
        {
            if (isGrounded)
            {
                verticalVelocity = jumpVelocity;
                isGrounded = false;
                currentState = PlayerState.Jumping;
                SetColliderNormal();

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayJump();
                }
            }
        }

        private void Slide()
        {
            if (!isGrounded)
            {
                // Fast dive downwards if in air
                verticalVelocity = -gravity * 0.7f;
            }

            slideTimer = slideDuration;
            currentState = PlayerState.Sliding;
            SetColliderSlide();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySlide();
            }
        }

        private void UpdateSpeed()
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);

            // Report distance to ScoreManager
            float deltaZ = currentSpeed * Time.deltaTime;
            float speedMult = currentSpeed / minSpeed;
            ScoreManager.Instance?.UpdateDistance(deltaZ, speedMult);
        }

        private void UpdateMovement()
        {
            // 1. Horizontal Lane Interpolation
            float targetX = targetLane * laneDistance;
            currentX = Mathf.MoveTowards(currentX, targetX, laneChangeSpeed * Time.deltaTime);

            // 2. Vertical Movement & Gravity
            float currentY = transform.position.y;
            if (!isGrounded)
            {
                verticalVelocity -= gravity * Time.deltaTime;
                currentY += verticalVelocity * Time.deltaTime;

                if (currentY <= 0f)
                {
                    currentY = 0f;
                    verticalVelocity = 0f;
                    isGrounded = true;
                    if (slideTimer > 0f) currentState = PlayerState.Sliding;
                    else currentState = PlayerState.Running;

                    CameraController.Instance?.Shake(0.06f);
                }
            }

            // 3. Slide Timer Countdown
            if (currentState == PlayerState.Sliding)
            {
                slideTimer -= Time.deltaTime;
                if (slideTimer <= 0f)
                {
                    slideTimer = 0f;
                    SetColliderNormal();
                    if (isGrounded) currentState = PlayerState.Running;
                }
            }

            // 4. Forward Z Movement
            float newZ = transform.position.z + currentSpeed * Time.deltaTime;

            transform.position = new Vector3(currentX, currentY, newZ);
        }

        private void UpdateMagnet()
        {
            // Strictly check directly ahead in current lane ONLY (narrow X bound 0.6m)
            Vector3 boxCenter = transform.position + Vector3.forward * 1.2f;
            Vector3 halfExtents = new Vector3(0.6f, 1.5f, 1.2f);
            Collider[] colliders = Physics.OverlapBox(boxCenter, halfExtents, Quaternion.identity);
            for (int i = 0; i < colliders.Length; i++)
            {
                var orb = colliders[i].GetComponent<CollectibleOrb>();
                if (orb != null)
                {
                    orb.TriggerMagnet(transform);
                }
            }
        }

        private void UpdateAnimation()
        {
            // Calculate lane lean angle based on horizontal movement
            float targetX = targetLane * laneDistance;
            float xDiff = targetX - currentX;
            float leanAngle = Mathf.Clamp(xDiff * -maxLeanAngle, -maxLeanAngle, maxLeanAngle);

            playerAnimator?.Animate(currentState, currentSpeed, leanAngle);
        }

        private void SetColliderNormal()
        {
            if (playerCollider != null)
            {
                playerCollider.height = normalHeight;
                playerCollider.center = normalCenter;
            }
        }

        private void SetColliderSlide()
        {
            if (playerCollider != null)
            {
                playerCollider.height = slideHeight;
                playerCollider.center = slideCenter;
            }
        }

        public void OnObstacleHit(Obstacle obstacle)
        {
            if (isDead) return;
            Die();
        }

        private void Die()
        {
            isDead = true;
            currentState = PlayerState.Crashed;
            currentSpeed = 0f;

            CameraController.Instance?.Shake(0.35f);
            CameraController.Instance?.TriggerCrashCam();

            AudioManager.Instance?.PlayCrash();
            playerAnimator?.Animate(PlayerState.Crashed, 0f, 0f);

            GameManager.Instance?.OnPlayerCrash();
        }
    }
}
