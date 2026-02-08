using System;
using UnityEngine;

public class HealthOrange : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int currentHp = 100;

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

    private float lastDamageTime = -999f;

    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;

    /// <summary>
    /// Fired when damage is applied. Args: (appliedDamage, currentHp, maxHp)
    /// </summary>
    public event Action<int, int, int> Damaged;

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
                animator.SetInteger(hitVariantIntName, amount);
            }
            animator.SetTrigger(hitTriggerName);
        }

        Damaged?.Invoke(applied, currentHp, maxHp);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        if (currentHp <= 0) return;

        currentHp = Mathf.Min(maxHp, currentHp + amount);
    }
}
