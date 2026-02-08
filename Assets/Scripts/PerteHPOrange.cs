using UnityEngine;

public class PerteHPOrange : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthOrange health;
    [SerializeField] private Transform bar;

    private float initialLocalWidth;
    private float initialRightLocalX;

    void Awake()
    {
        if (bar == null) bar = transform;
        if (health == null) health = FindFirstObjectByType<HealthOrange>();

        // Mirror of the blue bar: keep the RIGHT edge fixed in LOCAL space.
        initialLocalWidth = Mathf.Abs(bar.localScale.x);
        initialRightLocalX = bar.localPosition.x + (initialLocalWidth * 0.5f);
    }

    void Update()
    {
        if (health == null)
            return;

        float ratio = (health.MaxHp <= 0) ? 0f : (float)health.CurrentHp / health.MaxHp;
        ratio = Mathf.Clamp01(ratio);

        float targetLocalWidth = initialLocalWidth * ratio;
        SetLocalWidth(bar, targetLocalWidth);

        // Keep RIGHT edge aligned (mirror)
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
