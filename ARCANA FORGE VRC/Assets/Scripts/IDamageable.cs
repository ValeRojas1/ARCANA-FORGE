// Damageable.cs
using UnityEngine;
using System;

public interface IDamageable
{
    void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitNormal);
    bool IsAlive { get; }
}

public class Damageable : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private bool destroyOnDeath = false;
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;

    public event Action OnDamaged;
    public event Action OnDied;

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitNormal)
    {
        if (!IsAlive) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnDamaged?.Invoke();

        if (CurrentHealth == 0)
        {
            OnDied?.Invoke();
            if (destroyOnDeath) Destroy(gameObject);
        }
    }
}
