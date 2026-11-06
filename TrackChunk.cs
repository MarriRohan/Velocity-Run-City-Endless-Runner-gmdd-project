using System.Collections.Generic;
using UnityEngine;

namespace NeonVelocity
{
    public enum ChunkTheme
    {
        StandardCity,
        SkybridgeOverhead,
        HighwayOverpass,
        MetroStation,
        TunnelArch
    }

    public class TrackChunk : MonoBehaviour
    {
        [Header("Chunk Properties")]
        [SerializeField] private float chunkLength = 40.0f;
        [SerializeField] private ChunkTheme theme = ChunkTheme.StandardCity;

        [Header("Decorations")]
        [SerializeField] private GameObject skybridgeObject;
        [SerializeField] private GameObject overpassObject;
        [SerializeField] private GameObject metroEntranceObject;
        [SerializeField] private GameObject tunnelArchObject;

        private readonly List<GameObject> activeSpawns = new List<GameObject>();

        public float Length => chunkLength;
        public ChunkTheme Theme => theme;

        public void SetTheme(ChunkTheme newTheme)
        {
            theme = newTheme;

            if (skybridgeObject != null) skybridgeObject.SetActive(theme == ChunkTheme.SkybridgeOverhead);
            if (overpassObject != null) overpassObject.SetActive(theme == ChunkTheme.HighwayOverpass);
            if (metroEntranceObject != null) metroEntranceObject.SetActive(theme == ChunkTheme.MetroStation);
            if (tunnelArchObject != null) tunnelArchObject.SetActive(theme == ChunkTheme.TunnelArch);
        }

        public void AddSpawnedItem(GameObject item)
        {
            if (item != null)
            {
                activeSpawns.Add(item);
            }
        }

        public void ClearSpawnedItems()
        {
            for (int i = 0; i < activeSpawns.Count; i++)
            {
                if (activeSpawns[i] != null && activeSpawns[i].activeSelf)
                {
                    // Check if obstacle or orb and return to pool
                    var obstacle = activeSpawns[i].GetComponent<Obstacle>();
                    if (obstacle != null && ObjectPooler.Instance != null)
                    {
                        ObjectPooler.Instance.ReturnToPool(obstacle.PoolTag, activeSpawns[i]);
                        continue;
                    }

                    var orb = activeSpawns[i].GetComponent<CollectibleOrb>();
                    if (orb != null && ObjectPooler.Instance != null)
                    {
                        ObjectPooler.Instance.ReturnToPool(orb.PoolTag, activeSpawns[i]);
                        continue;
                    }

                    activeSpawns[i].SetActive(false);
                }
            }
            activeSpawns.Clear();
        }
    }
}
