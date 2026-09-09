using System.Collections;
using UnityEngine;
using Weapons;

public class Pistol : Weapon
{
    [Header("Combat Settings")]
    [SerializeField] private GameObject bulletPrefab; // Префаб пули
    [SerializeField] private Transform firePoint;     // Точка, откуда вылетает пуля (дуло)
    [SerializeField] private float bulletSpeed = 20f; // Скорость пули

    private int currentAmmo;
    private bool isReloading;
    private float nextFireTime;
    private ProjectileFactory projectileFactory;
    private ProjectilePool projectilePool;

    private void Start()
    {
        if (weaponData != null)
        {
            currentAmmo = weaponData.maxAmmo;
        }

        projectilePool = bulletPrefab != null
            ? new ProjectilePool(bulletPrefab)
            : null;
        projectileFactory = new ProjectileFactory(bulletPrefab, bulletSpeed, projectilePool);
    }

    public override void Attack()
    {
        if (weaponData == null)
            return;

        if (isReloading)
            return;

        if (Time.time < nextFireTime)
            return;

        if (currentAmmo <= 0)
        {
            Reload();
            return;
        }

        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Pistol: Не назначен BulletPrefab или FirePoint в инспекторе!");
            return;
        }

        currentAmmo--;
        nextFireTime = Time.time + weaponData.fireRate;

        projectileFactory.Spawn(firePoint, (int)weaponData.damage);

        Debug.Log($"Pistol fired! Ammo: {currentAmmo}/{weaponData.maxAmmo}");
    }

    public override void Reload()
    {
        if (isReloading || (weaponData != null && currentAmmo == weaponData.maxAmmo))
            return;

        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        float reloadTime = weaponData != null ? weaponData.reloadTime : 1.5f;
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = weaponData != null ? weaponData.maxAmmo : 12;
        isReloading = false;
        Debug.Log("Reload complete!");
    }
}