using UnityEngine;

/// <summary>
/// Simple in-scene teleporter: when a Player enters this trigger,
/// the player is moved to a destination Transform.
/// 
/// Usage:
/// - Put this script on a GameObject with a 2D Collider set as trigger.
/// - Assign a destination Transform (empty GameObject placed where you want to teleport).
/// - Ensure the player GameObject has tag "Player" (or change requiredTag).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PositionTeleporter : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform destination;

    [Header("Filter")]
    [Tooltip("Only objects with this tag will be teleported.")]
    [SerializeField] private string requiredTag = "Player";

    [Header("Behavior")]
    [Tooltip("Optional: small offset applied after teleport.")]
    [SerializeField] private Vector2 positionOffset;

    [Tooltip("If true, also clears the Rigidbody2D velocity after teleport.")]
    [SerializeField] private bool resetVelocity = true;

    [Tooltip("Seconds before this teleporter can be triggered again (prevents double-trigger spam).")]
    [Min(0f)]
    [SerializeField] private float cooldownSeconds = 0.1f;

    private float lastTeleportTime = -999f;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!string.IsNullOrWhiteSpace(requiredTag) && !other.CompareTag(requiredTag))
            return;

        if (destination == null)
        {
            Debug.LogError("[PositionTeleporter] Destination is not set.", this);
            return;
        }

        if (cooldownSeconds > 0f && Time.time < lastTeleportTime + cooldownSeconds)
            return;

        lastTeleportTime = Time.time;

        Vector3 targetPos = destination.position + (Vector3)positionOffset;

        // Move root transform (so you don't move only a child collider).
        Transform root = other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform;
        root.position = targetPos;

        if (resetVelocity && other.attachedRigidbody != null)
            other.attachedRigidbody.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (destination == null)
            return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, destination.position);
        Gizmos.DrawWireSphere(destination.position + (Vector3)positionOffset, 0.15f);
    }
}
