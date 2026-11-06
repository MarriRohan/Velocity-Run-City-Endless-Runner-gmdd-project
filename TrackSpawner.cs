using System.Collections.Generic;
using UnityEngine;

namespace NeonVelocity
{
    public class TrackSpawner : MonoBehaviour
    {
        public static TrackSpawner Instance { get; private set; }

        [Header("Track Configuration")]
        [SerializeField] private GameObject chunkPrefab;
        [SerializeField] private int initialChunks = 8;
        [SerializeField] private float chunkLength = 40.0f;
        [SerializeField] private float safeStartDistance = 60.0f; // No obstacles on very first section

        [Header("Pool Tags")]
        [SerializeField] private string tagLowBarricade = "Obstacle_LowBarricade";
        [SerializeField] private string tagHighGantry = "Obstacle_HighGantry";
        [SerializeField] private string tagCargoContainer = "Obstacle_CargoContainer";
        [SerializeField] private string tagHoverCar = "Obstacle_HoverCar";
        [SerializeField] private string tagOrb = "EnergyOrb";

        private readonly List<TrackChunk> activeChunks = new List<TrackChunk>();
        private float nextSpawnZ = 0f;
        private Transform playerTransform;

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
            if (PlayerController.Instance != null)
            {
                playerTransform = PlayerController.Instance.transform;
            }

            InitializeTrack();
        }

        public void InitializeTrack()
        {
            // Clear any existing chunks
            for (int i = 0; i < activeChunks.Count; i++)
            {
                if (activeChunks[i] != null)
                {
                    activeChunks[i].ClearSpawnedItems();
                    Destroy(activeChunks[i].gameObject);
                }
            }
            activeChunks.Clear();

            nextSpawnZ = -chunkLength; // Start slightly behind player

            for (int i = 0; i < initialChunks; i++)
            {
                SpawnChunk(isInitial: (i < 2));
            }
        }

        public void ResetTrack()
        {
            for (int i = 0; i < activeChunks.Count; i++)
            {
                if (activeChunks[i] != null)
                {
                    activeChunks[i].ClearSpawnedItems();
                }
            }

            nextSpawnZ = -chunkLength;
            for (int i = 0; i < activeChunks.Count; i++)
            {
                var chunk = activeChunks[i];
                chunk.transform.position = new Vector3(0f, 0f, nextSpawnZ);
                chunk.SetTheme((ChunkTheme)(i % 5));

                if (nextSpawnZ >= safeStartDistance)
                {
                    PopulateChunk(chunk);
                }
                nextSpawnZ += chunkLength;
            }
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                if (PlayerController.Instance != null) playerTransform = PlayerController.Instance.transform;
                else return;
            }

            // Check if the oldest chunk is far behind the player
            if (activeChunks.Count > 0)
            {
                TrackChunk oldestChunk = activeChunks[0];
                if (playerTransform.position.z - oldestChunk.transform.position.z > chunkLength * 1.5f)
                {
                    RecycleChunk(oldestChunk);
                }
            }

            // Update district progression in CityDistrictManager
            if (CityDistrictManager.Instance != null)
            {
                CityDistrictManager.Instance.CheckDistanceProgression(playerTransform.position.z);
            }
        }

        private void SpawnChunk(bool isInitial)
        {
            if (chunkPrefab == null) return;

            GameObject chunkObj = Instantiate(chunkPrefab, new Vector3(0f, 0f, nextSpawnZ), Quaternion.identity, transform);
            TrackChunk chunk = chunkObj.GetComponent<TrackChunk>();
            if (chunk == null) chunk = chunkObj.AddComponent<TrackChunk>();

            // Cycle themes across chunks
            int themeIdx = (activeChunks.Count % 5);
            chunk.SetTheme((ChunkTheme)themeIdx);

            if (!isInitial && nextSpawnZ >= safeStartDistance)
            {
                PopulateChunk(chunk);
            }

            activeChunks.Add(chunk);
            nextSpawnZ += chunkLength;
        }

        private void RecycleChunk(TrackChunk chunk)
        {
            chunk.ClearSpawnedItems();

            chunk.transform.position = new Vector3(0f, 0f, nextSpawnZ);

            // Assign varying themes (Skybridges, Overpasses, Metro, Tunnels)
            int themeIdx = Random.Range(0, 5);
            chunk.SetTheme((ChunkTheme)themeIdx);

            PopulateChunk(chunk);

            // Move to end of active list
            activeChunks.RemoveAt(0);
            activeChunks.Add(chunk);

            nextSpawnZ += chunkLength;
        }

        private void PopulateChunk(TrackChunk chunk)
        {
            if (ObjectPooler.Instance == null) return;

            // Each 40m chunk has two obstacle rows: at zOffset = 10m and 28m
            SpawnObstacleRow(chunk, 10.0f);
            SpawnObstacleRow(chunk, 28.0f);
        }

        private void SpawnObstacleRow(TrackChunk chunk, float localZ)
        {
            float chunkWorldZ = chunk.transform.position.z + localZ;
            float[] laneX = new float[] { -3.5f, 0f, 3.5f };

            // Decide pattern:
            // 0: 1 lane blocked (Low barricade), 2 lanes open with orbs
            // 1: 1 lane blocked (High gantry), 2 lanes open with orbs
            // 2: 1 lane blocked (Cargo container), 2 lanes open with orbs
            // 3: 2 lanes blocked (e.g. low barricade + cargo), 1 lane clear
            // 4: Moving hover car in 1 lane, jump/slide in another
            int pattern = Random.Range(0, 5);

            int blockedLane1 = Random.Range(0, 3);
            int blockedLane2 = (blockedLane1 + Random.Range(1, 3)) % 3;

            switch (pattern)
            {
                case 0: // Low Barricade (Jump)
                    SpawnObstacle(tagLowBarricade, ObstacleType.LowBarricade, laneX[blockedLane1], chunkWorldZ, chunk);
                    SpawnOrbJumpArc(laneX[blockedLane1], chunkWorldZ, chunk);
                    SpawnOrbLine(laneX[(blockedLane1 + 1) % 3], chunkWorldZ, chunk);
                    break;

                case 1: // High Gantry (Slide)
                    SpawnObstacle(tagHighGantry, ObstacleType.HighGantry, laneX[blockedLane1], chunkWorldZ, chunk);
                    SpawnOrbSlideLine(laneX[blockedLane1], chunkWorldZ, chunk);
                    SpawnOrbLine(laneX[(blockedLane1 + 2) % 3], chunkWorldZ, chunk);
                    break;

                case 2: // Block Obstacle (Cargo Container)
                    SpawnObstacle(tagCargoContainer, ObstacleType.CargoContainer, laneX[blockedLane1], chunkWorldZ, chunk);
                    SpawnOrbLine(laneX[(blockedLane1 + 1) % 3], chunkWorldZ, chunk);
                    break;

                case 3: // 2 lanes blocked, 1 open lane guaranteed!
                    SpawnObstacle(tagLowBarricade, ObstacleType.LowBarricade, laneX[blockedLane1], chunkWorldZ, chunk);
                    SpawnObstacle(tagCargoContainer, ObstacleType.CargoContainer, laneX[blockedLane2], chunkWorldZ, chunk);
                    // Open lane gets an orb line
                    int openLane = 3 - blockedLane1 - blockedLane2;
                    SpawnOrbLine(laneX[openLane], chunkWorldZ, chunk);
                    break;

                case 4: // Moving hover car in 1 lane
                    SpawnObstacle(tagHoverCar, ObstacleType.MovingCar, laneX[blockedLane1], chunkWorldZ + 8f, chunk);
                    SpawnObstacle(tagHighGantry, ObstacleType.HighGantry, laneX[blockedLane2], chunkWorldZ, chunk);
                    SpawnOrbSlideLine(laneX[blockedLane2], chunkWorldZ, chunk);
                    break;
            }
        }

        private void SpawnObstacle(string tag, ObstacleType type, float x, float z, TrackChunk chunk)
        {
            GameObject obj = ObjectPooler.Instance.SpawnFromPool(tag, new Vector3(x, 0f, z), Quaternion.identity);
            if (obj != null)
            {
                var obs = obj.GetComponent<Obstacle>();
                if (obs != null) obs.SetType(type, tag);
                chunk.AddSpawnedItem(obj);
            }
        }

        private void SpawnOrbLine(float x, float centerZ, TrackChunk chunk)
        {
            for (int i = -2; i <= 2; i++)
            {
                Vector3 pos = new Vector3(x, 0.85f, centerZ + i * 2.2f);
                GameObject orb = ObjectPooler.Instance.SpawnFromPool(tagOrb, pos, Quaternion.identity);
                if (orb != null) chunk.AddSpawnedItem(orb);
            }
        }

        private void SpawnOrbSlideLine(float x, float centerZ, TrackChunk chunk)
        {
            // Low line for sliding under high obstacles
            for (int i = -2; i <= 2; i++)
            {
                Vector3 pos = new Vector3(x, 0.45f, centerZ + i * 2.0f);
                GameObject orb = ObjectPooler.Instance.SpawnFromPool(tagOrb, pos, Quaternion.identity);
                if (orb != null) chunk.AddSpawnedItem(orb);
            }
        }

        private void SpawnOrbJumpArc(float x, float centerZ, TrackChunk chunk)
        {
            // Parabolic jump arc over low obstacles
            for (int i = -2; i <= 2; i++)
            {
                float t = i / 2.0f; // -1 to 1
                float arcY = 0.85f + (1.0f - t * t) * 1.6f; // Peak at 2.45m
                Vector3 pos = new Vector3(x, arcY, centerZ + i * 2.2f);
                GameObject orb = ObjectPooler.Instance.SpawnFromPool(tagOrb, pos, Quaternion.identity);
                if (orb != null) chunk.AddSpawnedItem(orb);
            }
        }
    }
}
