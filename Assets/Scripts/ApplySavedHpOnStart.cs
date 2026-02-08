using UnityEngine;

/// <summary>
/// Put this in every gameplay scene (or on each player prefab).
/// It applies saved HP from GameState to the current Health components.
/// </summary>
public class ApplySavedHpOnStart : MonoBehaviour
{
    private void Start()
    {
        // If GameState hasn't been created yet, this will create it (default HPs = 100).
        var state = GameState.Instance;

        var bleu = FindFirstObjectByType<HealthBleu>();
        var orange = FindFirstObjectByType<HealthOrange>();

        if (bleu != null)
            state.ApplyTo(bleu);

        if (orange != null)
            state.ApplyTo(orange);
    }
}
