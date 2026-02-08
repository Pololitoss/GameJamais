using UnityEngine;

public class HealthBleu : MonoBehaviour
{
    [SerializeField] private int maxHp = 5;
    [SerializeField] private int currentHp;

    [Tooltip("Désactive les dégâts pendant X secondes après un hit.")]
    [SerializeField] private float invulnerabilitySeconds = 0.1f;

    private float invulnerableUntil;

    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;
    public bool IsDead => currentHp <= 0;

    public event System.Action<int> Damaged; // amount

    private void Awake()
    {
        if (maxHp < 1) maxHp = 1;
        if (currentHp <= 0) currentHp = maxHp;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        if (Time.time < invulnerableUntil) return;
        if (IsDead) return;

        int before = currentHp;
        currentHp = Mathf.Max(0, currentHp - amount);
        invulnerableUntil = Time.time + Mathf.Max(0f, invulnerabilitySeconds);

        int applied = before - currentHp;
        if (applied > 0)
            Damaged?.Invoke(applied);

        if (IsDead)
        {
            // TODO: gérer mort du bleu
        }
    }
}
