using System.Collections;
using UnityEngine;
using Weapons; // Добавили пространство имен для Bullet

public class Pistol : Weapon
{
    [Header("Combat Settings")]
    [SerializeField] private GameObject bulletPrefab; // Префаб пули
    [SerializeField] private Transform firePoint;     // Точка, откуда вылетает пуля (дуло)
    [SerializeField] private float bulletSpeed = 20f; // Скорость пули

    private int currentAmmo;
    private bool isReloading;
    private float nextFireTime;

    private void Start()
    {
        if (weaponData != null)
        {
            currentAmmo = weaponData.maxAmmo;
        }
    }

    public override void Attack()
    {
        if (isReloading)
            return;

        if (Time.time < nextFireTime)
            return;

        if (currentAmmo <= 0)
        {
            Reload();
            return;
        }

        currentAmmo--;
        nextFireTime = Time.time + weaponData.fireRate;

        SpawnBullet();

        Debug.Log($"Pistol fired! Ammo: {currentAmmo}/{weaponData.maxAmmo}");
    }

    private void SpawnBullet()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Pistol: Не назначен BulletPrefab или FirePoint в инспекторе!");
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Передаем урон пуле (с явным приведением к int, если damage это float)
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            int pistolDamage = weaponData != null ? (int)weaponData.damage : 20; 
            bulletScript.SetDamage(pistolDamage);
        }

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.right * bulletSpeed;
        }
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