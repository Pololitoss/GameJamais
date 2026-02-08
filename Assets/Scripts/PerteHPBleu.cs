using UnityEngine;

public class PerteHPBleu : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Référence vers la vie du joueur Bleu.")]
    [SerializeField] private HealthBleu health;

    [Header("Optional: set if the bar is not this object")]
    [SerializeField]
    private Transform bar;

    [Header("Death menu (shown when HP reaches 0)")]
    [SerializeField]
    private DeathMenuController deathMenu;

    private bool isDead;

    private float initialLocalWidth;
    private float currentLocalWidth;
    private float initialLeftLocalX;

    void Awake()
    {
        if (bar == null)
            bar = transform;

        if (health == null)
            health = GetComponentInParent<HealthBleu>();

        // IMPORTANT: comme la barre peut tourner, on ne doit PAS utiliser
        // r.bounds.size.x (qui est en WORLD et dépend de la rotation).
        // On travaille en LOCAL: la "largeur" = scale.x de l'objet.
        initialLocalWidth = Mathf.Abs(bar.localScale.x);
        currentLocalWidth = initialLocalWidth;

        // Keep the LEFT edge fixed in LOCAL space.
        // Assumes the sprite is centered on its Transform.
        initialLeftLocalX = bar.localPosition.x - (currentLocalWidth * 0.5f);
    }

    void Update()
    {
        if (health == null)
            return;

        float ratio = (health.MaxHp > 0) ? Mathf.Clamp01((float)health.CurrentHp / health.MaxHp) : 1f;
        currentLocalWidth = initialLocalWidth * ratio;
        ApplyWidthAndAlignLeft(currentLocalWidth);

        if (!isDead && health.CurrentHp <= 0)
        {
            isDead = true;
            if (deathMenu != null)
                deathMenu.Show();
        }
    }

    private void ApplyWidthAndAlignLeft(float targetLocalWidth)
    {
        SetLocalWidth(bar, targetLocalWidth);

        Vector3 p = bar.localPosition;
        p.x = initialLeftLocalX + (targetLocalWidth * 0.5f);
        bar.localPosition = p;
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
}
