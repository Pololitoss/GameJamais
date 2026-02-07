using UnityEngine;
using UnityEngine.InputSystem;

public class PerteHPBleu : MonoBehaviour
{
    [Header("How much to shrink each click (world units)")]
    [Tooltip("Amount removed from the bar width each click, in world units (e.g. 0.1)")]
    [SerializeField]
    private float shrinkWorldUnits = 0.3f;

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
        // Unity 6 project is using the new Input System.
        // Right click -> Blue takes damage.
        // Works even when legacy Input is disabled.
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            ShrinkOnce();
        }
    }

    private void ShrinkOnce()
    {
        // Réduit la largeur d'une taille constante dans l'espace LOCAL.
        // (C'est stable même si l'objet tourne.)
        currentLocalWidth = Mathf.Max(0f, currentLocalWidth - Mathf.Max(0f, shrinkWorldUnits));
        SetLocalWidth(bar, currentLocalWidth);

        // Re-align to the left: keep left edge constant, move center accordingly.
        Vector3 p = bar.localPosition;
        p.x = initialLeftLocalX + (currentLocalWidth * 0.5f);
        bar.localPosition = p;

        if (!isDead && currentLocalWidth <= 0f)
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
}
