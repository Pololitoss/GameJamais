using UnityEngine;
using UnityEngine.InputSystem;

public class PivotHpOnClick : MonoBehaviour
{
    [Header("Target (optional)")]
    [SerializeField] private Transform bar; // si vide => ce GameObject

    [Header("Rotation")]
    [Tooltip("Degrees added each click (e.g. 15). Can be negative.")]
    [SerializeField] private float degreesPerClick = 15f;

    [Tooltip("If true, rotation is smoothed over time instead of instant.")]
    [SerializeField] private bool smoothRotate = true;

    [Tooltip("Rotation speed (degrees per second) when smoothRotate is true.")]
    [Min(1f)]
    [SerializeField] private float rotateSpeed = 360f;

    [Tooltip("If true, rotates around Z (2D). If false, uses Y (3D-style).")]
    [SerializeField] private bool rotateAroundZ = true;

    [Header("Glitch options")]
    [Tooltip("If > 0, adds a random extra rotation in range [-random, +random].")]
    [SerializeField] private float randomDegrees = 15f;

    [Tooltip("If true, smoothly returns back to the original rotation.")]
    [SerializeField] private bool returnToStart = false;

    [Tooltip("How fast it returns (degrees per second). Only if returnToStart is true.")]
    [SerializeField] private float returnSpeed = 180f;

    [Header("Glitch move")]
    [Tooltip("Random offset radius applied on click (local units).")]
    [SerializeField] private float moveRadius = 0.15f;

    [Tooltip("If true, move is smoothed over time instead of teleporting.")]
    [SerializeField] private bool smoothMove = true;

    [Tooltip("How fast the bar reaches its target position (units per second).")]
    [Min(0.01f)]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Keep on screen")]
    [Tooltip("Padding in viewport space (0..0.5). 0.1 means keep center inside [0.1, 0.9].")]
    [Range(0f, 0.5f)]
    [SerializeField] private float viewportPadding = 0.08f;

    [Tooltip("If true, smoothly returns back to the original position.")]
    [SerializeField] private bool returnToStartPosition = false;

    [Tooltip("How fast it returns to position (higher = faster).")]
    [SerializeField] private float returnPosSpeed = 8f;

    private Quaternion startRotation;
    private Vector3 pivotWorld;
    private Vector3 startLocalPos;
    private Rigidbody2D rb2d;

    private bool hasMoveTarget;
    private Vector2 moveTargetWorld;

    private bool hasRotateTarget;
    private float rotateRemainingDegrees;

    void Awake()
    {
        if (bar == null) bar = transform;
        startRotation = bar.rotation;
        startLocalPos = bar.localPosition;

        rb2d = bar.GetComponent<Rigidbody2D>();

        // Use the visual center as pivot (works even if the Transform pivot isn't centered)
        pivotWorld = GetVisualCenterWorld(bar);
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Recompute pivot each click (the object can move/scale/rotate)
            pivotWorld = GetVisualCenterWorld(bar);

            float extra = (randomDegrees > 0f) ? Random.Range(-randomDegrees, randomDegrees) : 0f;
            float total = degreesPerClick + extra;

            if (smoothRotate)
            {
                rotateRemainingDegrees += total;
                hasRotateTarget = true;
            }
            else
            {
                Vector3 axis = rotateAroundZ ? Vector3.forward : Vector3.up;
                bar.RotateAround(pivotWorld, axis, total);
            }

            // Random glitch move (local)
            if (moveRadius > 0f)
            {
                Vector2 rnd = Random.insideUnitCircle * moveRadius;

                // We work in WORLD space for clamp correctness.
                Vector2 currentWorld = (rb2d != null) ? rb2d.position : (Vector2)bar.position;
                moveTargetWorld = ClampToViewport(currentWorld + rnd);
                hasMoveTarget = true;

                if (!smoothMove)
                {
                    ApplyMoveWorld(moveTargetWorld);
                    hasMoveTarget = false;
                }
            }
        }

        if (hasRotateTarget && smoothRotate)
        {
            // Rotate smoothly around the (updated) visual center.
            pivotWorld = GetVisualCenterWorld(bar);

            float maxStep = rotateSpeed * Time.deltaTime;
            float step = Mathf.Clamp(rotateRemainingDegrees, -maxStep, maxStep);

            // For 2D sprites we rotate around Z (Vector3.forward).
            Vector3 axis = rotateAroundZ ? Vector3.forward : Vector3.up;
            bar.RotateAround(pivotWorld, axis, step);

            rotateRemainingDegrees -= step;
            if (Mathf.Abs(rotateRemainingDegrees) <= 0.0001f)
            {
                rotateRemainingDegrees = 0f;
                hasRotateTarget = false;
            }
        }

        if (hasMoveTarget && smoothMove)
        {
            Vector2 currentWorld = (rb2d != null) ? rb2d.position : (Vector2)bar.position;
            Vector2 next = Vector2.MoveTowards(currentWorld, moveTargetWorld, moveSpeed * Time.deltaTime);
            ApplyMoveWorld(next);

            if ((moveTargetWorld - next).sqrMagnitude <= 0.000001f)
                hasMoveTarget = false;
        }

        if (returnToStart)
        {
            bar.rotation = Quaternion.RotateTowards(bar.rotation, startRotation, returnSpeed * Time.deltaTime);
        }

        if (returnToStartPosition)
        {
            // Smooth exponential-like lerp, framerate independent
            float t = 1f - Mathf.Exp(-returnPosSpeed * Time.deltaTime);

            if (rb2d != null)
                rb2d.MovePosition(Vector2.Lerp(rb2d.position, (Vector2)startLocalPos, t));
            else
                bar.localPosition = Vector3.Lerp(bar.localPosition, startLocalPos, t);
        }
    }

    // Optionnel: si tu veux remettre à zéro depuis un bouton / autre script
    public void ResetRotation()
    {
        bar.rotation = startRotation;
    }

    private Vector3 GetVisualCenterWorld(Transform t)
    {
        // SpriteRenderer / MeshRenderer etc.
        var r = t.GetComponent<Renderer>();
        if (r == null)
            r = t.GetComponentInChildren<Renderer>();

        if (r != null)
            return r.bounds.center;

        // Fallback: transform position
        return t.position;
    }

    private Vector2 ClampToViewport(Vector2 targetWorldPos)
    {
        Camera cam = Camera.main;
        if (cam == null)
            return targetWorldPos;

        // Keep the VISUAL center inside the viewport.
        // We approximate by clamping the transform position itself.
        Vector3 vp = cam.WorldToViewportPoint(new Vector3(targetWorldPos.x, targetWorldPos.y, 0f));
        vp.x = Mathf.Clamp(vp.x, viewportPadding, 1f - viewportPadding);
        vp.y = Mathf.Clamp(vp.y, viewportPadding, 1f - viewportPadding);
        Vector3 world = cam.ViewportToWorldPoint(vp);
        return new Vector2(world.x, world.y);
    }

    private void ApplyMoveWorld(Vector2 worldPos)
    {
        if (rb2d != null)
        {
            // For platforms moved by script: set Rigidbody2D to Kinematic in the inspector.
            rb2d.MovePosition(worldPos);
        }
        else
        {
            bar.position = new Vector3(worldPos.x, worldPos.y, bar.position.z);
        }
    }
}
