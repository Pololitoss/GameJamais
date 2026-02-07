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

        // Save return info before changing scenes.
        TimedSceneReturnService.StartTimedReturn(SceneManager.GetActiveScene().name, durationSeconds);

        // Ensure timeScale isn't stuck at 0 from pause/death.
        Time.timeScale = 1f;

        if (destroyOnTrigger)
            Destroy(gameObject);

        SceneManager.LoadScene(targetSceneName);
    }
}
