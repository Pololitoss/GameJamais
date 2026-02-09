using System;
using UnityEngine;

public class HealthBleu : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHp = 300;
    [SerializeField] private int currentHp = 300;

    [Header("Invulnerability")]
    [Tooltip("Seconds of invulnerability after taking a hit.")]
    [SerializeField] private float invulnSeconds = 0.1f;

    [Header("Hit reaction (optional)")]
    [Tooltip("Animator used to play a hit reaction. If null, it will try GetComponent<Animator>() on this GameObject.")]
    [SerializeField] private Animator animator;

    [Tooltip("Trigger name used to play the hit reaction animation.")]
    [SerializeField] private string hitTriggerName = "Hit";

    [Tooltip("Optional int parameter name if you want multiple hit reactions (0/1/2...). Leave empty to disable.")]
    [SerializeField] private string hitVariantIntName = "";

    [Header("Death reaction (optional)")]
    [Tooltip("Trigger name used to play the death animation.")]
    [SerializeField] private string dieTriggerName = "Die";

    [Tooltip("Bool parameter set to true when dead, to prevent death animation from being cancelled.")]
    [SerializeField] private string isDeadBoolName = "IsDead";

    private float lastDamageTime = -999f;

    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;

    /// <summary>
    /// Fired when damage is applied. Args: (appliedDamage, currentHp, maxHp)
    /// </summary>
    public event Action<int, int, int> Damaged;

    /// <summary>
    /// Fired once when HP reaches 0.
    /// </summary>
    public event Action Died;

    private bool hasDied;

    public bool IsDead => currentHp <= 0;

    private void Awake()
    {
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (currentHp <= 0) return;

        if (Time.time < lastDamageTime + Mathf.Max(0f, invulnSeconds))
            return;

        lastDamageTime = Time.time;

        int before = currentHp;
        currentHp = Mathf.Max(0, currentHp - amount);
        int applied = before - currentHp;

        // Play hit reaction animation
        if (applied > 0 && animator != null && !string.IsNullOrWhiteSpace(hitTriggerName))
        {
            if (!string.IsNullOrWhiteSpace(hitVariantIntName))
            {
                // Example: choose variant based on damage amount
                animator.SetInteger(hitVariantIntName, amount);
            }
            animator.SetTrigger(hitTriggerName);
        }

        Damaged?.Invoke(applied, currentHp, maxHp);

        if (!hasDied && currentHp <= 0)
        {
            hasDied = true;
            if (animator != null && !string.IsNullOrWhiteSpace(dieTriggerName))
                animator.SetTrigger(dieTriggerName);
            if (animator != null && !string.IsNullOrWhiteSpace(isDeadBoolName))
                animator.SetBool(isDeadBoolName, true);
            Died?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        if (currentHp <= 0) return;

        currentHp = Mathf.Min(maxHp, currentHp + amount);
    }

    /// <summary>
    /// Used by persistent state systems to restore HP after a scene load.
    /// </summary>
    public void SetHp(int newCurrentHp, int newMaxHp)
    {
        maxHp = Mathf.Max(1, newMaxHp);
        currentHp = Mathf.Clamp(newCurrentHp, 0, maxHp);

        hasDied = currentHp <= 0;

        if (animator != null && !string.IsNullOrWhiteSpace(isDeadBoolName))
            animator.SetBool(isDeadBoolName, currentHp <= 0);
    }
}
