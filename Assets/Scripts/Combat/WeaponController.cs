public sealed class WeaponController
{
    private readonly Weapon weapon;

    public WeaponController(Weapon weapon)
    {
        this.weapon = weapon;
    }

    public void Attack()
    {
        weapon?.Attack();
    }

    public void Reload()
    {
        weapon?.Reload();
    }
}