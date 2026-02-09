using UnityEngine;

/// <summary>
/// Simple persistent runtime state for the duel.
/// Stores HP so it can survive scene loads.
/// </summary>
public class GameState : MonoBehaviour
{
    private static GameState instance;

    [Header("HP state")]
    [SerializeField] private int bleuMaxHp = 300;
    [SerializeField] private int bleuCurrentHp = 300;
    [SerializeField] private int orangeMaxHp = 300;
    [SerializeField] private int orangeCurrentHp = 300;

    [Tooltip("Becomes true once SaveBleu/SaveOrange has been called. Until then, we won't overwrite scene/prefab HP values.")]
    [SerializeField] private bool hasSavedHp;

    public static GameState Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("GameState");
                instance = go.AddComponent<GameState>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveBleu(HealthBleu h)
    {
        if (h == null) return;
        bleuMaxHp = h.MaxHp;
        bleuCurrentHp = h.CurrentHp;
        hasSavedHp = true;
    }

    public void SaveBleu(int currentHp, int maxHp)
    {
        bleuMaxHp = Mathf.Max(1, maxHp);
        bleuCurrentHp = Mathf.Clamp(currentHp, 0, bleuMaxHp);
        hasSavedHp = true;
    }

    public void SaveOrange(HealthOrange h)
    {
        if (h == null) return;
        orangeMaxHp = h.MaxHp;
        orangeCurrentHp = h.CurrentHp;
        hasSavedHp = true;
    }

    public void SaveOrange(int currentHp, int maxHp)
    {
        orangeMaxHp = Mathf.Max(1, maxHp);
        orangeCurrentHp = Mathf.Clamp(currentHp, 0, orangeMaxHp);
        hasSavedHp = true;
    }

    public void ApplyTo(HealthBleu h)
    {
        if (h == null) return;
        if (!hasSavedHp) return;
        h.SetHp(bleuCurrentHp, bleuMaxHp);
    }

    public void ApplyTo(HealthOrange h)
    {
        if (h == null) return;
        if (!hasSavedHp) return;
        h.SetHp(orangeCurrentHp, orangeMaxHp);
    }

    /// <summary>
    /// Called when starting a new run (e.g. restart from death menu).
    /// Clears saved HP so players start from their scene/prefab values again.
    /// </summary>
    public void ClearSavedHp()
    {
        hasSavedHp = false;
    }

    /// <summary>
    /// Resets the persistent HP store to the default values configured on this GameState.
    /// Used for "new game" / restart flows.
    /// </summary>
    public void ResetHpToDefaults()
    {
        // Keep the configured defaults (serialized fields), just mark as saved so Apply works deterministically.
        bleuCurrentHp = bleuMaxHp;
        orangeCurrentHp = orangeMaxHp;
        hasSavedHp = true;
    }
}
