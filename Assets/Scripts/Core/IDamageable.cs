public interface IDamageable
{
    void TakeDamage(float damage);
    float CurrentHealth { get; }
}
