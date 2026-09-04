using UnityEngine;
using System;

public class CharacterStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private int maxJumps = 1;

    private float currentHealth;

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
    }

    private void Start()
    {
        // Сброс всех модификаторов при старте (для новой игры)
        modifiedMaxHealth.RemoveAllModifiers();
        modifiedMoveSpeed.RemoveAllModifiers();
        modifiedJumpForce.RemoveAllModifiers();
        modifiedMaxJumps.RemoveAllModifiers();

        currentHealth = MaxHealth;
    }

    public void AddStatModifier(StatType statType, StatModifier modifier)
    {
        switch (statType)
        {
            case StatType.MaxHealth:
                modifiedMaxHealth.AddModifier(modifier);
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

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth < 0f)
            currentHealth = 0f;

        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0f)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;

        if (currentHealth > MaxHealth)
            currentHealth = MaxHealth;

        OnHealthChanged?.Invoke(currentHealth);
    }
}
