using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    [Serializable]
    public class PoolDefinition
    {
        public string key;            
        public GameObject prefab;
        public int initialSize;
    }

    public class PoolManager : MonoSingleton<PoolManager>
    {
        [SerializeField] private List<PoolDefinition> pools = new();

        private readonly Dictionary<string, Queue<GameObject>> _poolQueues = new();
        private readonly Dictionary<string, GameObject> _prefabLookup = new();
        private readonly Dictionary<string, Transform> _poolParents = new();
        
        public void InitializePools()
        {
            foreach (var def in pools)
            {
                if (def.prefab == null || string.IsNullOrWhiteSpace(def.key))
                    continue;
                if (_poolQueues.ContainsKey(def.key))
                    continue;

                var queue = new Queue<GameObject>(def.initialSize);
                _poolQueues[def.key] = queue;
                _prefabLookup[def.key] = def.prefab;

                var parent = new GameObject($"[{def.key}] Pool");
                parent.transform.SetParent(transform);
                _poolParents[def.key] = parent.transform;

                for (int i = 0; i < def.initialSize; i++)
                {
                    var obj = Instantiate(def.prefab, parent.transform);
                    obj.SetActive(false);
                    queue.Enqueue(obj);
                }
            }
        }

        public GameObject Spawn(string key, Vector3 position, Quaternion rotation)
        {
            if (!_prefabLookup.ContainsKey(key))
            {
                Debug.LogError($"[PoolManager] Unknown pool key: {key}");
                return null;
            }

            if (!_poolQueues.TryGetValue(key, out var queue))
            {
                queue = new Queue<GameObject>();
                _poolQueues[key] = queue;
            }

            GameObject obj;
            if (queue.Count > 0)
                obj = queue.Dequeue();
            else
                obj = Instantiate(_prefabLookup[key], _poolParents[key]);

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        public void Despawn(string key, GameObject obj)
        {
            if (!_poolQueues.ContainsKey(key))
            {
                obj.SetActive(false);
                Debug.LogWarning($"[PoolManager] Despawn for unknown key {key}. Object disabled.");
                return;
            }

            obj.SetActive(false);
            obj.transform.SetParent(_poolParents[key], worldPositionStays: false);
            _poolQueues[key].Enqueue(obj);
        }
    }
}
