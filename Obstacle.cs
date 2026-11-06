using UnityEngine;

namespace NeonVelocity
{
    public enum ObstacleType
    {
        LowBarricade,    // Player jumps over
        HighGantry,      // Player slides under
        CargoContainer,  // Player changes lane
        MovingCar        // Drives toward player in lane
    }

    public class Obstacle : MonoBehaviour, IPooledObject
    {
        [Header("Obstacle Settings")]
        [SerializeField] private ObstacleType obstacleType;
        [SerializeField] private float moveSpeed = 7.0f; // Speed moving obstacles travel toward player
        [SerializeField] private string poolTag = "Obstacle";

        private Vector3 initialLocalPos;
        private bool isMoving;

        public ObstacleType Type => obstacleType;
        public string PoolTag { get => poolTag; set => poolTag = value; }

        public void SetType(ObstacleType type, string tag)
        {
            obstacleType = type;
            poolTag = tag;
        }

        public void OnObjectSpawn()
        {
            isMoving = (obstacleType == ObstacleType.MovingCar);
            initialLocalPos = transform.localPosition;
        }

        private void Update()
        {
            if (isMoving && GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                // Move towards player (-Z relative to world/track)
                transform.position += Vector3.back * (moveSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var player = other.GetComponent<PlayerController>();
                if (player != null && !player.IsDead)
                {
                    player.OnObstacleHit(this);
                }
            }
        }
    }
}
