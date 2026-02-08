using UnityEngine;

/// <summary>
/// Simple persistent runtime state for the duel.
/// Stores HP so it can survive scene loads.
/// </summary>
public class GameState : MonoBehaviour
{
    private static GameState instance;

    [Header("HP state")]
    [SerializeField] private int bleuMaxHp = 100;
    [SerializeField] private int bleuCurrentHp = 100;
    [SerializeField] private int orangeMaxHp = 100;
    [SerializeField] private int orangeCurrentHp = 100;

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
    }

    public void SaveOrange(HealthOrange h)
    {
        if (h == null) return;
        orangeMaxHp = h.MaxHp;
        orangeCurrentHp = h.CurrentHp;
    }

    public void ApplyTo(HealthBleu h)
    {
        if (h == null) return;
        h.SetHp(bleuCurrentHp, bleuMaxHp);
    }

    public void ApplyTo(HealthOrange h)
    {
        if (h == null) return;
        h.SetHp(orangeCurrentHp, orangeMaxHp);
    }
}
