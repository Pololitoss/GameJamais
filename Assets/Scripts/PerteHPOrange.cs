using UnityEngine;

public class PerteHPOrange : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Référence vers la vie du joueur Orange.")]
    [SerializeField] private HealthOrange health;

    [Header("Optional: set if the bar is not this object")]
    [SerializeField]
    private Transform bar;

    [Header("Death menu (optional)")]
    [SerializeField] private DeathMenuController deathMenu;

    private bool isDead;

    private float initialLocalWidth;
    private float currentLocalWidth;
    private float initialRightLocalX;

    void Awake()
    {
        if (bar == null)
            bar = transform;

        if (health == null)
            health = GetComponentInParent<HealthOrange>();

        // Mirror of the blue bar: keep the RIGHT edge fixed in LOCAL space.
        // (Rotation-safe: do NOT use Renderer.bounds which is world-aligned.)
        initialLocalWidth = Mathf.Abs(bar.localScale.x);
        currentLocalWidth = initialLocalWidth;

        // Assumes the sprite is centered on its Transform.
        initialRightLocalX = bar.localPosition.x + (currentLocalWidth * 0.5f);
    }

    void Update()
    {
        if (health == null)
            return;

        float ratio = (health.MaxHp > 0) ? Mathf.Clamp01((float)health.CurrentHp / health.MaxHp) : 1f;
        currentLocalWidth = initialLocalWidth * ratio;
        ApplyWidthAndAlignRight(currentLocalWidth);

        if (!isDead && health.CurrentHp <= 0)
        {
            isDead = true;
            if (deathMenu != null)
                deathMenu.Show();
        }
    }

    private void ApplyWidthAndAlignRight(float targetLocalWidth)
    {
        SetLocalWidth(bar, targetLocalWidth);

        Vector3 p = bar.localPosition;
        p.x = initialRightLocalX - (targetLocalWidth * 0.5f);
        bar.localPosition = p;
    }

    private void SetLocalWidth(Transform t, float targetLocalWidth)
    {
        Vector3 s = t.localScale;
        float sign = Mathf.Sign(s.x);
        if (sign == 0f) sign = 1f;

        s.x = sign * targetLocalWidth;
        t.localScale = s;
    }
}
