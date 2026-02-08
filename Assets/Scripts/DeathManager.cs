using UnityEngine;

/// <summary>
/// Scene-level manager: shows the same death menu when either player dies.
/// Drop this on a GameObject (e.g. "DeathManager") in gameplay scenes.
/// </summary>
public class DeathManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private DeathMenuController deathMenu;

    [Header("Players (optional)")]
    [SerializeField] private HealthBleu bleu;
    [SerializeField] private HealthOrange orange;

    [Header("Timing")]
    [Tooltip("Seconds to wait (real-time) before showing the death menu, so the death animation can play.")]
    [Min(0f)]
    [SerializeField] private float showMenuDelaySeconds = 1.2f;

    private bool shown;
    private float showAtRealtime = -1f;

    private void Awake()
    {
        if (deathMenu == null)
            deathMenu = FindFirstObjectByType<DeathMenuController>();

        if (bleu == null)
            bleu = FindFirstObjectByType<HealthBleu>();

        if (orange == null)
            orange = FindFirstObjectByType<HealthOrange>();
    }

    private void OnEnable()
    {
        if (bleu != null) bleu.Died += OnAnyDied;
        if (orange != null) orange.Died += OnAnyDied;
    }

    private void OnDisable()
    {
        if (bleu != null) bleu.Died -= OnAnyDied;
        if (orange != null) orange.Died -= OnAnyDied;
    }

    private void Update()
    {
        if (shown) return;
        if (showAtRealtime < 0f) return;

        if (Time.realtimeSinceStartup >= showAtRealtime)
        {
            shown = true;
            showAtRealtime = -1f;

            if (deathMenu != null)
                deathMenu.Show();
            else
                Debug.LogError("[DeathManager] No DeathMenuController found in scene.", this);
        }
    }

    private void OnAnyDied()
    {
        // Start (or restart) the countdown.
        showAtRealtime = Time.realtimeSinceStartup + Mathf.Max(0f, showMenuDelaySeconds);
    }
}
