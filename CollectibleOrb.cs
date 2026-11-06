using UnityEngine;

namespace NeonVelocity
{
    public class CollectibleOrb : MonoBehaviour, IPooledObject
    {
        [Header("Coin Visuals")]
        [SerializeField] private float rotationSpeed = 160.0f;
        [SerializeField] private float bobFrequency = 2.5f;
        [SerializeField] private float bobAmplitude = 0.12f;
        [SerializeField] private int scoreValue = 100;
        [SerializeField] private string poolTag = "EnergyOrb";

        [Header("Lane Bounds")]
        [SerializeField] private float maxCollectionLateralDistance = 1.1f; // Strictly in same lane

        private Vector3 startLocalPos;
        private bool isAttracted = false;
        private Transform playerTarget;
        private float seedOffset;

        public string PoolTag { get => poolTag; set => poolTag = value; }

        public void OnObjectSpawn()
        {
            isAttracted = false;
            playerTarget = null;
            startLocalPos = transform.localPosition;
            seedOffset = Random.Range(0f, 10f);
        }

        private void Update()
        {
            if (isAttracted && playerTarget != null)
            {
                // Only attract if still in same lane
                if (Mathf.Abs(transform.position.x - playerTarget.position.x) > maxCollectionLateralDistance)
                {
                    isAttracted = false;
                    playerTarget = null;
                }
                else
                {
                    Vector3 targetPos = playerTarget.position + Vector3.up * 1.0f;
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, 20.0f * Time.deltaTime);

                    if (Vector3.Distance(transform.position, targetPos) < 0.6f)
                    {
                        Collect();
                    }
                    return;
                }
            }

            // Spinning gold coin
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            float newY = startLocalPos.y + Mathf.Sin((Time.time + seedOffset) * bobFrequency) * bobAmplitude;
            transform.localPosition = new Vector3(startLocalPos.x, newY, startLocalPos.z);
        }

        public void TriggerMagnet(Transform player)
        {
            if (player == null) return;
            // CRITICAL: ONLY allow attraction if player is directly in the same lane!
            if (Mathf.Abs(transform.position.x - player.position.x) > maxCollectionLateralDistance)
            {
                return;
            }

            if (isAttracted) return;
            isAttracted = true;
            playerTarget = player;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Strictly ensure player is in the same straight lane, not in an adjacent lane
                float lateralDist = Mathf.Abs(transform.position.x - other.transform.position.x);
                if (lateralDist <= maxCollectionLateralDistance)
                {
                    Collect();
                }
            }
        }

        private void Collect()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddEnergyOrb(scoreValue);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOrbCollect();
            }

            if (ObjectPooler.Instance != null)
            {
                ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
