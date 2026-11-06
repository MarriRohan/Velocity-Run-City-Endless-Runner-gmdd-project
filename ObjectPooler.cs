using System.Collections.Generic;
using UnityEngine;

namespace NeonVelocity
{
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [System.Serializable]
        public class PoolItem
        {
            public string tag;
            public GameObject prefab;
            public int prewarmCount = 10;
        }

        [Header("Pool Configurations")]
        [SerializeField] private List<PoolItem> poolDefinitions = new List<PoolItem>();

        private readonly Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();
        private Transform poolContainer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            poolContainer = new GameObject("_ObjectPoolContainer").transform;
            poolContainer.SetParent(transform);

            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var item in poolDefinitions)
            {
                if (item.prefab == null || string.IsNullOrEmpty(item.tag)) continue;

                prefabLookup[item.tag] = item.prefab;
                if (!poolDictionary.ContainsKey(item.tag))
                {
                    poolDictionary[item.tag] = new Queue<GameObject>();
                }

                for (int i = 0; i < item.prewarmCount; i++)
                {
                    GameObject obj = Instantiate(item.prefab, poolContainer);
                    obj.name = $"{item.tag}_{i}";
                    obj.SetActive(false);
                    poolDictionary[item.tag].Enqueue(obj);
                }
            }
        }

        public void RegisterDynamicPool(string tag, GameObject prefab, int prewarmCount)
        {
            if (prefabLookup.ContainsKey(tag)) return;

            prefabLookup[tag] = prefab;
            if (!poolDictionary.ContainsKey(tag))
            {
                poolDictionary[tag] = new Queue<GameObject>();
            }

            if (poolContainer == null)
            {
                poolContainer = new GameObject("_ObjectPoolContainer").transform;
                poolContainer.SetParent(transform);
            }

            for (int i = 0; i < prewarmCount; i++)
            {
                GameObject obj = Instantiate(prefab, poolContainer);
                obj.name = $"{tag}_{i}";
                obj.SetActive(false);
                poolDictionary[tag].Enqueue(obj);
            }
        }

        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag) || poolDictionary[tag] == null)
            {
                Debug.LogWarning($"Pool with tag '{tag}' does not exist.");
                return null;
            }

            GameObject objToSpawn;
            if (poolDictionary[tag].Count > 0)
            {
                objToSpawn = poolDictionary[tag].Dequeue();
            }
            else
            {
                // Grow pool if exhausted
                if (prefabLookup.TryGetValue(tag, out GameObject prefab))
                {
                    objToSpawn = Instantiate(prefab, poolContainer);
                    objToSpawn.name = $"{tag}_dynamic";
                }
                else
                {
                    return null;
                }
            }

            objToSpawn.transform.SetPositionAndRotation(position, rotation);
            objToSpawn.SetActive(true);

            IPooledObject pooled = objToSpawn.GetComponent<IPooledObject>();
            pooled?.OnObjectSpawn();

            return objToSpawn;
        }

        public void ReturnToPool(string tag, GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);
            obj.transform.SetParent(poolContainer);

            if (!poolDictionary.ContainsKey(tag))
            {
                poolDictionary[tag] = new Queue<GameObject>();
            }

            poolDictionary[tag].Enqueue(obj);
        }
    }

    public interface IPooledObject
    {
        void OnObjectSpawn();
    }
}
