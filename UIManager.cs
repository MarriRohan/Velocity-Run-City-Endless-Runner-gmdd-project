using UnityEngine;
using UnityEngine.UI;

namespace NeonVelocity
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("HUD Elements")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text distanceText;
        [SerializeField] private Text districtText;
        [SerializeField] private Text orbText;
        [SerializeField] private Text speedText;
        [SerializeField] private Image speedGaugeBar;
        [SerializeField] private Text districtNotificationText;

        [Header("Main Menu Elements")]
        [SerializeField] private Text menuBestScoreText;
        [SerializeField] private Button playButton;

        [Header("Game Over Elements")]
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text finalDistanceText;
        [SerializeField] private Text finalOrbsText;
        [SerializeField] private GameObject newRecordBadge;
        [SerializeField] private Button runAgainButton;
        [SerializeField] private Button menuButton;

        private float notificationTimer = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            SetupButtonListeners();
        }

        private void SetupButtonListeners()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (runAgainButton != null) runAgainButton.onClick.AddListener(OnRunAgainClicked);
            if (menuButton != null) menuButton.onClick.AddListener(OnMenuClicked);
        }

        public void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (hudPanel != null) hudPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            if (menuBestScoreText != null && ScoreManager.Instance != null)
            {
                menuBestScoreText.text = $"BEST RECORD: {ScoreManager.Instance.BestScore:D6}";
            }
        }

        public void ShowHUD()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(true);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
        }

        public void ShowGameOver()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (hudPanel != null) hudPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(true);

            if (ScoreManager.Instance != null)
            {
                if (finalScoreText != null) finalScoreText.text = $"{ScoreManager.Instance.CurrentScore:D6}";
                if (bestScoreText != null) bestScoreText.text = $"{ScoreManager.Instance.BestScore:D6}";
                if (finalDistanceText != null) finalDistanceText.text = $"DISTANCE: {Mathf.FloorToInt(ScoreManager.Instance.CurrentDistance)} M   |   COINS: {ScoreManager.Instance.CoinsCollected}";
                if (finalOrbsText != null) finalOrbsText.text = $"{ScoreManager.Instance.CoinsCollected}";
                if (newRecordBadge != null) newRecordBadge.SetActive(ScoreManager.Instance.IsNewRecord);
            }
        }

        public void ShowDistrictNotification(string districtName)
        {
            if (districtNotificationText != null)
            {
                districtNotificationText.text = $">> ENTERING: {districtName} <<";
                districtNotificationText.gameObject.SetActive(true);
                notificationTimer = 3.5f;
            }
        }

        private void Update()
        {
            if (notificationTimer > 0f)
            {
                notificationTimer -= Time.deltaTime;
                if (notificationTimer <= 0f && districtNotificationText != null)
                {
                    districtNotificationText.gameObject.SetActive(false);
                }
            }

            if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                UpdateHUD();
            }
        }

        private void UpdateHUD()
        {
            if (ScoreManager.Instance != null)
            {
                if (scoreText != null) scoreText.text = $"SCORE: {ScoreManager.Instance.CurrentScore:D6}";
                if (distanceText != null) distanceText.text = $"DIST: {Mathf.FloorToInt(ScoreManager.Instance.CurrentDistance)} M";
                if (orbText != null) orbText.text = $"COINS: {ScoreManager.Instance.EnergyOrbsCollected}";
            }

            if (CityDistrictManager.Instance != null && districtText != null)
            {
                districtText.text = CityDistrictManager.Instance.CurrentDistrictName;
            }

            if (PlayerController.Instance != null)
            {
                float speedKmh = PlayerController.Instance.SpeedKmh;
                if (speedText != null) speedText.text = $"{Mathf.RoundToInt(speedKmh)} KM/H";

                if (speedGaugeBar != null)
                {
                    float minKmh = PlayerController.Instance.MinSpeed * 3.6f;
                    float maxKmh = PlayerController.Instance.MaxSpeed * 3.6f;
                    speedGaugeBar.fillAmount = Mathf.InverseLerp(minKmh, maxKmh, speedKmh);
                }
            }
        }

        private void OnPlayClicked()
        {
            AudioManager.Instance?.PlayUIClick();
            GameManager.Instance?.StartGame();
        }

        private void OnRunAgainClicked()
        {
            AudioManager.Instance?.PlayUIClick();
            GameManager.Instance?.RestartRun();
        }

        private void OnMenuClicked()
        {
            AudioManager.Instance?.PlayUIClick();
            GameManager.Instance?.ReturnToMenu();
        }
    }
}
