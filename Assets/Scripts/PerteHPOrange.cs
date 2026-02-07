using UnityEngine;
using UnityEngine.InputSystem;

public class PerteHPOrange : MonoBehaviour
{
    [Header("How much to shrink each click (world units)")]
    [Tooltip("Amount removed from the bar width each click, in world units (e.g. 0.1)")]
    [SerializeField]
    private float shrinkWorldUnits = 0.3f;

    [Header("Optional: set if the bar is not this object")]
    [SerializeField]
    private Transform bar;

    private float initialLocalWidth;
    private float currentLocalWidth;
    private float initialRightLocalX;

    void Awake()
    {
        if (bar == null)
            bar = transform;

        // Mirror of the blue bar: keep the RIGHT edge fixed in LOCAL space.
        // (Rotation-safe: do NOT use Renderer.bounds which is world-aligned.)
        initialLocalWidth = Mathf.Abs(bar.localScale.x);
        currentLocalWidth = initialLocalWidth;

        // Assumes the sprite is centered on its Transform.
        initialRightLocalX = bar.localPosition.x + (currentLocalWidth * 0.5f);
    }

    void Update()
    {
        // Unity 6 project is using the new Input System.
        // This checks for a left mouse click and works even when legacy Input is disabled.
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShrinkOnce();
        }
    }

    private void ShrinkOnce()
    {
        // Shrink by a constant amount in LOCAL space (stable even when rotating)
        currentLocalWidth = Mathf.Max(0f, currentLocalWidth - Mathf.Max(0f, shrinkWorldUnits));
        SetLocalWidth(bar, currentLocalWidth);

        // Keep RIGHT edge aligned (mirror): move center accordingly.
        Vector3 p = bar.localPosition;
        p.x = initialRightLocalX - (currentLocalWidth * 0.5f);
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
