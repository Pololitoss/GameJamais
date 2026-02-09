using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Put this on a pickup/portal object. When the Player touches it, it loads a target scene
/// for a given duration, then returns to the original scene.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TimedSceneTeleport : MonoBehaviour
{
    [Header("Teleport")]
    [Tooltip("Scene name to load when the player picks up this object.")]
    [SerializeField] private string targetSceneName;

    [Tooltip("How long (in seconds) the player stays in the target scene.")]
    [Min(0.1f)]
    [SerializeField] private float durationSeconds = 20f;

    [Tooltip("Destroy this pickup object after triggering.")]
    [SerializeField] private bool destroyOnTrigger = true;

    private bool triggered;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError("[TimedSceneTeleport] targetSceneName is empty.", this);
            return;
        }

        triggered = true;

        // Persist HP before changing scenes.
        // Use all instances and keep the lowest CurrentHp found (avoids picking a fresh/full duplicate).
        int bleuCur = int.MaxValue, bleuMax = 0;
        foreach (var b in FindObjectsByType<HealthBleu>(FindObjectsSortMode.None))
        {
            if (b == null) continue;
            bleuCur = Mathf.Min(bleuCur, b.CurrentHp);
            bleuMax = Mathf.Max(bleuMax, b.MaxHp);
        }

        int orangeCur = int.MaxValue, orangeMax = 0;
        foreach (var o in FindObjectsByType<HealthOrange>(FindObjectsSortMode.None))
        {
            if (o == null) continue;
            orangeCur = Mathf.Min(orangeCur, o.CurrentHp);
            orangeMax = Mathf.Max(orangeMax, o.MaxHp);
        }

        if (bleuCur != int.MaxValue)
            GameState.Instance.SaveBleu(bleuCur, bleuMax);

        if (orangeCur != int.MaxValue)
            GameState.Instance.SaveOrange(orangeCur, orangeMax);

        // Save return info before changing scenes.
        TimedSceneReturnService.StartTimedReturn(SceneManager.GetActiveScene().name, durationSeconds);

        // Ensure timeScale isn't stuck at 0 from pause/death.
        Time.timeScale = 1f;

        if (destroyOnTrigger)
            Destroy(gameObject);

        SceneManager.LoadScene(targetSceneName);
    }
}
