using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NeonVelocity
{
    public enum GameState
    {
        Menu,
        Playing,
        GameOver,
        Paused
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.Menu;

        public GameState State => currentState;
        public bool IsPlaying => currentState == GameState.Playing;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            ReturnToMenu();
        }

        private void Update()
        {
            bool startPressed = false;

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
                {
                    startPressed = true;
                }
            }
            var pad = Gamepad.current;
            if (pad != null && pad.buttonSouth.wasPressedThisFrame)
            {
                startPressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                startPressed = true;
            }
#endif

            if (startPressed)
            {
                if (currentState == GameState.Menu)
                {
                    StartGame();
                }
                else if (currentState == GameState.GameOver)
                {
                    RestartRun();
                }
            }
        }

        public void StartGame()
        {
            currentState = GameState.Playing;

            ScoreManager.Instance?.ResetScore();
            CityDistrictManager.Instance?.ResetDistricts();
            PlayerController.Instance?.ResetPlayer();
            TrackSpawner.Instance?.ResetTrack();

            if (PlayerController.Instance != null && CameraController.Instance != null)
            {
                CameraController.Instance.SetTarget(PlayerController.Instance.transform);
            }

            UIManager.Instance?.ShowHUD();
            AudioManager.Instance?.PlayBGM();
        }

        public void OnPlayerCrash()
        {
            currentState = GameState.GameOver;

            ScoreManager.Instance?.SaveScore();
            UIManager.Instance?.ShowGameOver();
        }

        public void RestartRun()
        {
            StartGame();
        }

        public void ReturnToMenu()
        {
            currentState = GameState.Menu;

            ScoreManager.Instance?.ResetScore();
            PlayerController.Instance?.ResetPlayer();
            TrackSpawner.Instance?.ResetTrack();

            UIManager.Instance?.ShowMainMenu();
            AudioManager.Instance?.StopBGM();
        }
    }
}
