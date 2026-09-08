public static class DamageSystem
{
    public static bool Apply(IDamageable target, float damage)
    {
        if (target == null || damage <= 0f)
            return false;

        target.TakeDamage(damage);
        return true;
    }
}