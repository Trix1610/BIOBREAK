using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName;
    public WeaponType weaponType;

    [Header("Stats")]
    public float damage = 10f;
    public float fireRate = 0.5f;
    public int maxAmmo = 12;
    public float reloadTime = 1.5f;
}

public enum WeaponType
{
    Pistol,
    Rifle,
    Shotgun,
    Melee
}
