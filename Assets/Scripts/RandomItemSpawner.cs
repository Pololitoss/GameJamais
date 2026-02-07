using System;
using UnityEngine;

public class RandomItemSpawner : MonoBehaviour
{
    [Serializable]
    public class DropEntry
    {
        public GameObject prefab;
        [Min(0f)] public float weight = 1f;
    }

    [Header("What can drop")]
    [SerializeField] private DropEntry[] drops;

    [Header("Where to spawn")]
    [Tooltip("Spawner will pick a random point inside this BoxCollider2D bounds.")]
    [SerializeField] private BoxCollider2D spawnArea;

    [Header("When to spawn")]
    [Tooltip("If true, spawns automatically every interval.")]
    [SerializeField] private bool spawnOnTimer = true;

    [Min(0.1f)]
    [SerializeField] private float spawnIntervalSeconds = 5f;

    [Tooltip("Maximum number of spawned items kept in the scene (older ones are destroyed).")]
    [Min(0)]
    [SerializeField] private int maxAlive = 20;

    [Header("Placement")]
    [Tooltip("Z value for spawned objects (useful in 2D so items are visible).")]
    [SerializeField] private float spawnZ = 0f;

    [Tooltip("Try multiple times to find a non-overlapping spot.")]
    [Min(1)]
    [SerializeField] private int tries = 10;

    [Tooltip("If > 0, avoids spawning too close to colliders on these layers.")]
    [SerializeField] private float avoidRadius = 0.2f;

    [SerializeField] private LayerMask avoidLayers;

    private float timer;
    private readonly System.Collections.Generic.Queue<GameObject> alive = new();

    private void Reset()
    {
        spawnArea = GetComponent<BoxCollider2D>();
    }

    private void Awake()
    {
        if (spawnArea == null)
            spawnArea = GetComponent<BoxCollider2D>();

        // Usually spawn areas should be triggers.
        if (spawnArea != null)
            spawnArea.isTrigger = true;
    }

    private void Update()
    {
        if (!spawnOnTimer) return;
        if (spawnIntervalSeconds <= 0f) return;

        timer += Time.deltaTime;
        if (timer >= spawnIntervalSeconds)
        {
            timer = 0f;
            SpawnOnce();
        }
    }

    public GameObject SpawnOnce()
    {
        var prefab = PickPrefab();
        if (prefab == null)
            return null;

        if (spawnArea == null)
        {
            Debug.LogError("[RandomItemSpawner] No spawnArea set (BoxCollider2D).", this);
            return null;
        }

        if (!TryPickPosition(out var pos))
            return null;

        var go = Instantiate(prefab, pos, Quaternion.identity);
        Track(go);
        return go;
    }

    private void Track(GameObject go)
    {
        if (go == null) return;

        alive.Enqueue(go);
        if (maxAlive <= 0) return;

        while (alive.Count > maxAlive)
        {
            var old = alive.Dequeue();
            if (old != null)
                Destroy(old);
        }
    }

    private bool TryPickPosition(out Vector3 pos)
    {
        Bounds b = spawnArea.bounds;

        for (int i = 0; i < tries; i++)
        {
            float x = UnityEngine.Random.Range(b.min.x, b.max.x);
            float y = UnityEngine.Random.Range(b.min.y, b.max.y);
            pos = new Vector3(x, y, spawnZ);

            if (avoidRadius <= 0f || avoidLayers.value == 0)
                return true;

            // If something is too close, retry.
            var hit = Physics2D.OverlapCircle(pos, avoidRadius, avoidLayers);
            if (hit == null)
                return true;
        }

        pos = default;
        return false;
    }

    private GameObject PickPrefab()
    {
        if (drops == null || drops.Length == 0)
            return null;

        float total = 0f;
        for (int i = 0; i < drops.Length; i++)
        {
            if (drops[i] == null || drops[i].prefab == null) continue;
            if (drops[i].weight <= 0f) continue;
            total += drops[i].weight;
        }

        if (total <= 0f)
            return null;

        float r = UnityEngine.Random.Range(0f, total);
        for (int i = 0; i < drops.Length; i++)
        {
            var d = drops[i];
            if (d == null || d.prefab == null) continue;
            if (d.weight <= 0f) continue;

            r -= d.weight;
            if (r <= 0f)
                return d.prefab;
        }

        // Fallback
        for (int i = 0; i < drops.Length; i++)
        {
            if (drops[i]?.prefab != null)
                return drops[i].prefab;
        }

        return null;
    }
}
