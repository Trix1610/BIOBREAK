using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponData weaponData;

    public WeaponData Data => weaponData;

    protected virtual void Awake()
    {
        if (weaponData == null)
        {
            Debug.LogError($"Weapon: WeaponData is not assigned on {gameObject.name}");
        }
    }

    public abstract void Attack();
    public abstract void Reload();
}
