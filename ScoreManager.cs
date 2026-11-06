using UnityEngine;

namespace NeonVelocity
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        private const string PREF_BEST_SCORE = "NEON_VELOCITY_BEST_SCORE";

        [Header("State")]
        [SerializeField] private int currentScore = 0;
        [SerializeField] private float currentDistance = 0f;
        [SerializeField] private int energyOrbsCollected = 0;
        [SerializeField] private int bestScore = 0;
        [SerializeField] private bool isNewRecord = false;

        public int CurrentScore => currentScore;
        public float CurrentDistance => currentDistance;
        public int EnergyOrbsCollected => energyOrbsCollected;
        public int CoinsCollected => energyOrbsCollected;
        public int BestScore => bestScore;
        public bool IsNewRecord => isNewRecord;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            bestScore = PlayerPrefs.GetInt(PREF_BEST_SCORE, 0);
        }

        public void ResetScore()
        {
            currentScore = 0;
            currentDistance = 0f;
            energyOrbsCollected = 0;
            isNewRecord = false;
        }

        public void UpdateDistance(float deltaMeters, float speedMultiplier)
        {
            currentDistance += deltaMeters;
            // 1 point per 2 meters, amplified by speed multiplier
            currentScore += Mathf.RoundToInt(deltaMeters * 0.5f * speedMultiplier);

            CheckNewRecord();
        }

        public void AddEnergyOrb(int points)
        {
            energyOrbsCollected++;
            currentScore += points;

            CheckNewRecord();
        }

        private void CheckNewRecord()
        {
            if (currentScore > bestScore)
            {
                bestScore = currentScore;
                isNewRecord = true;
            }
        }

        public void SaveScore()
        {
            if (currentScore >= bestScore)
            {
                bestScore = currentScore;
                PlayerPrefs.SetInt(PREF_BEST_SCORE, bestScore);
                PlayerPrefs.Save();
            }
        }
    }
}
