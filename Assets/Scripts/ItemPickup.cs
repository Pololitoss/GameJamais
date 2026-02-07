using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    public enum PickupType
    {
        HealBlue,
        HealOrange,
        Custom
    }

    [Header("Pickup")]
    [SerializeField] private PickupType type = PickupType.Custom;

    [Tooltip("Amount used by Heal pickups.")]
    [SerializeField] private float amount = 1f;

    [Tooltip("Destroy this object on pickup.")]
    [SerializeField] private bool destroyOnPickup = true;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Keep it generic: you can tag your player objects with "Player" or add a specific component.
        // For now, we accept anything tagged Player.
        if (!other.CompareTag("Player"))
            return;

        // TODO: plug into your HP system. Right now your HP is just a bar scale, not a real HP value.
        // This is a safe placeholder so you can see items get picked up.
        Debug.Log($"[ItemPickup] Picked up {type} (amount={amount}) by {other.name}", this);

        if (destroyOnPickup)
            Destroy(gameObject);
    }
}
