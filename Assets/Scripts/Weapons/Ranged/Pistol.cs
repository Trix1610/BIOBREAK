using UnityEngine;

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

        // Вызываем спавн пули
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

        // Создаем пулю в позиции и С ТЕКУЩИМ ПОВОРОТОМ FirePoint
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Пуля полетит в ту сторону, куда смотрит firePoint (его правая ось)
            rb.linearVelocity = firePoint.right * bulletSpeed;
        }
    }

    public override void Reload()
    {
        if (isReloading || (weaponData != null && currentAmmo == weaponData.maxAmmo))
            return;

        StartCoroutine(ReloadCoroutine());
    }

    private System.Collections.IEnumerator ReloadCoroutine()
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