using UnityEngine;

public class PerteHPBleu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthBleu health;
    [SerializeField] private Transform bar;

    [Header("Death menu (optional)")]
    [SerializeField] private DeathMenuController deathMenu;

    private bool isDead;
    private float initialLocalWidth;
    private float initialLeftLocalX;

    void Awake()
    {
        if (bar == null) bar = transform;
        if (health == null) health = FindBestHealthBleu();

        // Rotation-safe: do NOT use Renderer.bounds (world). Use localScale.x as width.
        initialLocalWidth = Mathf.Abs(bar.localScale.x);

        // Keep LEFT edge fixed in local space.
        initialLeftLocalX = bar.localPosition.x - (initialLocalWidth * 0.5f);
    }

    private void OnEnable()
    {
        // In some scenes, UI can enable before the correct player prefab finishes spawning.
        if (health == null)
            health = FindBestHealthBleu();
    }

    void Update()
    {
        if (health == null)
            return;

        float ratio = (health.MaxHp <= 0) ? 0f : (float)health.CurrentHp / health.MaxHp;
        ratio = Mathf.Clamp01(ratio);

        float targetLocalWidth = initialLocalWidth * ratio;
        SetLocalWidth(bar, targetLocalWidth);

        // Keep left edge aligned and adjust center.
        Vector3 p = bar.localPosition;
        p.x = initialLeftLocalX + (targetLocalWidth * 0.5f);
        bar.localPosition = p;

        if (!isDead && health.CurrentHp <= 0)
        {
            isDead = true;
            if (deathMenu != null)
                deathMenu.Show();
        }
    }

    private void SetLocalWidth(Transform t, float targetLocalWidth)
    {
        Vector3 s = t.localScale;
        float sign = Mathf.Sign(s.x);
        if (sign == 0f) sign = 1f;

        // Garde le sens du scale (si tu avais un scale négatif pour flip)
        s.x = sign * targetLocalWidth;
        t.localScale = s;
    }

    private HealthBleu FindBestHealthBleu()
    {
        HealthBleu best = null;
        int bestCurrent = int.MaxValue;

        foreach (var h in FindObjectsByType<HealthBleu>(FindObjectsSortMode.None))
        {
            if (h == null) continue;

            // If duplicates exist, prefer the one that's actually taking damage.
            if (h.CurrentHp < bestCurrent)
            {
                best = h;
                bestCurrent = h.CurrentHp;
            }
        }

        return best;
    }
}
