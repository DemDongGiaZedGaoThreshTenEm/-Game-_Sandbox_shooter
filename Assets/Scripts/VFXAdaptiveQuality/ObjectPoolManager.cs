using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    [System.Serializable]
    public class Pool
    {
        public string poolName;
        public GameObject prefab;
        public int size;
        public bool ApplyQuality;
    }

    [SerializeField] private List<Pool> pools;

    private Dictionary<string, Queue<GameObject>> poolDictionary;
    /// <summary>
    /// Danh sách tất cả Effect hỗ trợ thay đổi Quality.
    /// Được tạo một lần khi khởi tạo Pool.
    /// </summary>
    private readonly HashSet<IQualityScalable> qualityControllers =
        new HashSet<IQualityScalable>();    
    
    [SerializeField]
    private bool useObjectPooling = true;

    public bool HasPool(string poolName)
    {
        if (!useObjectPooling)
            return false;

        if (string.IsNullOrWhiteSpace(poolName))
            return false;

        return poolDictionary.ContainsKey(poolName);
    }    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (!useObjectPooling)
                return;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);

                obj.SetActive(false);

                bool hasParticle =
                    obj.GetComponentInChildren<ParticleSystem>(true) != null;

                bool hasVFX =
                    obj.GetComponentInChildren<VisualEffect>(true) != null;

                if (hasParticle || hasVFX)
                {
                    EffectQualityController controller =
                        obj.GetComponent<EffectQualityController>();

                    if (controller == null)
                    {
                        controller = obj.AddComponent<EffectQualityController>();
                        controller.Initialize();
                    }

                    if (pool.ApplyQuality)
                    {
                        qualityControllers.Add(controller);
                    }
                }

                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.poolName, objectPool);

            Debug.Log($"Pool [{pool.poolName}] created with {pool.size} objects.");
        }    
    }

    public GameObject SpawnFromPool(
        string poolName,
        Vector3 position,
        Quaternion rotation)
    {
        if (!useObjectPooling)
        return null;
        
        if (!poolDictionary.TryGetValue(poolName, out Queue<GameObject> pool))
        {
            Debug.LogWarning($"Pool {poolName} không tồn tại!");
            return null;
        }

        GameObject objectToSpawn = pool.Dequeue();

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        objectToSpawn.SetActive(false);
        objectToSpawn.SetActive(true);

        pool.Enqueue(objectToSpawn);

        return objectToSpawn;
    }
    /// Áp dụng Quality cho toàn bộ Effect trong Pool.
    /// Được QualityManager gọi.
    /// </summary>
    public void ApplyQuality(QualityLevel level)
    {
        int updated = 0;

        foreach (IQualityScalable controller in qualityControllers)
        {
            if (controller == null)
                continue;

            controller.ApplyQuality(level);
            updated++;
        }

        Debug.Log(
            $"[ObjectPoolManager] Applied {level} quality to {updated} controllers.");
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        if (!useObjectPooling || poolDictionary == null)
            return;

        foreach (Pool pool in pools)
        {
            UpdatePoolQuality(pool);
        }
    }

    private void UpdatePoolQuality(Pool pool)
    {
        if (!poolDictionary.TryGetValue(pool.poolName, out Queue<GameObject> objectPool))
            return;

        foreach (GameObject obj in objectPool)
        {
            if (obj == null)
                continue;

            EffectQualityController controller =
                obj.GetComponent<EffectQualityController>();

            if (controller == null)
                continue;

            if (pool.ApplyQuality)
            {
                qualityControllers.Add(controller);

                // Đồng bộ ngay quality hiện tại
                if (QualityManager.Instance != null)
                {
                    controller.ApplyQuality(
                        QualityManager.Instance.CurrentQuality);
                }
            }
            else
            {
                qualityControllers.Remove(controller);

                // Trả về mặc định (High)
                controller.ApplyQuality(QualityLevel.High);
            }
        }
    }
}