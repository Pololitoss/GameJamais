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

        // Apply to all instances in the scene (handles duplicates/disabled objects safely).
        foreach (var bleu in FindObjectsByType<HealthBleu>(FindObjectsSortMode.None))
            state.ApplyTo(bleu);

        foreach (var orange in FindObjectsByType<HealthOrange>(FindObjectsSortMode.None))
            state.ApplyTo(orange);
    }
}
