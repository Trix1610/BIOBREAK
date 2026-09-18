using UnityEngine;
using System;

public class CharacterStats : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private int maxJumps = 1;

    private float currentHealth;
    private bool isDead;

    private ModifiedStat modifiedMaxHealth;
    private ModifiedStat modifiedMoveSpeed;
    private ModifiedStat modifiedJumpForce;
    private ModifiedStat modifiedMaxJumps;

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    public float MaxHealth => modifiedMaxHealth.Value;
    public float CurrentHealth => currentHealth;
    public float MoveSpeed => modifiedMoveSpeed.Value;
    public float JumpForce => modifiedJumpForce.Value;
    public int MaxJumps => (int)modifiedMaxJumps.Value;

    private void Awake()
    {
        modifiedMaxHealth = new ModifiedStat(maxHealth);
        modifiedMoveSpeed = new ModifiedStat(moveSpeed);
        modifiedJumpForce = new ModifiedStat(jumpForce);
        modifiedMaxJumps = new ModifiedStat(maxJumps);

        currentHealth = MaxHealth;
        isDead = false;
    }

    private void Start()
    {
        // Сброс всех модификаторов при старте (для новой игры)
        modifiedMaxHealth.RemoveAllModifiers();
        modifiedMoveSpeed.RemoveAllModifiers();
        modifiedJumpForce.RemoveAllModifiers();
        modifiedMaxJumps.RemoveAllModifiers();

        currentHealth = MaxHealth;
        isDead = false;
    }

    public void AddStatModifier(StatType statType, StatModifier modifier)
    {
        switch (statType)
        {
            case StatType.MaxHealth:
                modifiedMaxHealth.AddModifier(modifier);
                ClampCurrentHealthToMax();
                break;
            case StatType.MoveSpeed:
                modifiedMoveSpeed.AddModifier(modifier);
                break;
            case StatType.JumpForce:
                modifiedJumpForce.AddModifier(modifier);
                break;
            case StatType.MaxJumps:
                modifiedMaxJumps.AddModifier(modifier);
                break;
        }
    }

    public void RemoveStatModifier(StatType statType, StatModifier modifier)
    {
        switch (statType)
        {
            case StatType.MaxHealth:
                modifiedMaxHealth.RemoveModifier(modifier);
                ClampCurrentHealthToMax();
                break;
            case StatType.MoveSpeed:
                modifiedMoveSpeed.RemoveModifier(modifier);
                break;
            case StatType.JumpForce:
                modifiedJumpForce.RemoveModifier(modifier);
                break;
            case StatType.MaxJumps:
                modifiedMaxJumps.RemoveModifier(modifier);
                break;
        }
    }

    public void RemoveStatModifiersFromSource(StatType statType, object source)
    {
        switch (statType)
        {
            case StatType.MaxHealth:
                modifiedMaxHealth.RemoveModifiersFromSource(source);
                ClampCurrentHealthToMax();
                break;
            case StatType.MoveSpeed:
                modifiedMoveSpeed.RemoveModifiersFromSource(source);
                break;
            case StatType.JumpForce:
                modifiedJumpForce.RemoveModifiersFromSource(source);
                break;
            case StatType.MaxJumps:
                modifiedMaxJumps.RemoveModifiersFromSource(source);
                break;
        }
    }

    // Перегрузка для int (чтобы принимать урон от врагов, передающих целые числа)
    public void TakeDamage(int damage)
    {
        TakeDamage((float)damage);
    }

    // Основной метод для float
    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        currentHealth -= damage;

        if (currentHealth < 0f)
            currentHealth = 0f;

        Debug.Log($"CharacterStats: получено урона {damage}. Осталось здоровья: {currentHealth}. Подписчиков на OnHealthChanged: {OnHealthChanged?.GetInvocationList().Length ?? 0}");
        OnHealthChanged?.Invoke(currentHealth);
        Debug.Log($"CharacterStats: событие OnHealthChanged вызвано");

        if (currentHealth <= 0f)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        Heal((float)amount);
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHealth += amount;

        if (currentHealth > MaxHealth)
            currentHealth = MaxHealth;

        OnHealthChanged?.Invoke(currentHealth);
    }

    private void ClampCurrentHealthToMax()
    {
        if (currentHealth <= MaxHealth)
            return;

        currentHealth = MaxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
}