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

    private bool bleuBound;
    private bool orangeBound;

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
        TryBindPlayers();
    }

    private void OnDisable()
    {
        UnbindPlayers();
    }

    private void Update()
    {
        // Some scenes instantiate players after this object is enabled.
        // Keep trying to bind until we have our subscriptions.
        if ((!bleuBound || !orangeBound) && !shown)
            TryBindPlayers();

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
        // If we are in a timed scene (ErrorMap), cancel the scheduled return.
        // Otherwise the game could load the previous scene while the death menu is shown.
        TimedSceneReturnService.Cancel();

        // Start (or restart) the countdown.
        showAtRealtime = Time.realtimeSinceStartup + Mathf.Max(0f, showMenuDelaySeconds);
    }

    private void TryBindPlayers()
    {
        if (deathMenu == null)
            deathMenu = FindFirstObjectByType<DeathMenuController>();

        if (deathMenu == null)
        {
            // Keep quiet-ish: lots of scenes won’t have a menu in editor while setting up, but it’s useful to know.
            // We only log once to avoid spam.
            Debug.LogWarning("[DeathManager] DeathMenuController not found (yet).", this);
        }

        if (!bleuBound)
        {
            if (bleu == null)
                bleu = FindFirstObjectByType<HealthBleu>();

            if (bleu != null)
            {
                bleu.Died += OnAnyDied;
                bleuBound = true;
            }
        }

        if (!orangeBound)
        {
            if (orange == null)
                orange = FindFirstObjectByType<HealthOrange>();

            if (orange != null)
            {
                orange.Died += OnAnyDied;
                orangeBound = true;
            }
        }
    }

    private void UnbindPlayers()
    {
        if (bleuBound && bleu != null)
            bleu.Died -= OnAnyDied;
        if (orangeBound && orange != null)
            orange.Died -= OnAnyDied;

        bleuBound = false;
        orangeBound = false;
    }
}
