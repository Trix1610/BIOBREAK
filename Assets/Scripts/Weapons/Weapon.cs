using UnityEngine;

public enum WeaponType
{
    Pistol,
    Rifle,
    Shotgun,
    MachineGun,
    Laser,
    Shock,
    Melee
}

public class Weapon : MonoBehaviour
{
    [Header("Weapon Info")]
    [SerializeField] private string weaponName = "Shotgun";
    [SerializeField] private WeaponType weaponType = WeaponType.Shotgun;

    [Header("Stats")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private int maxAmmo = 8;
    [SerializeField] private float reloadTime = 1.5f;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject bulletPrefab; 
    [SerializeField] private Transform firePoint;     
    [SerializeField] private float bulletSpeed = 20f; 

    [Header("Laser Settings")]
    [SerializeField] private float laserRange = 100f; 

    [Header("Shock/Melee Settings")]
    [SerializeField] private float shockRadius = 5f;   
    [SerializeField] private LayerMask shockTargetLayer; 

    [SerializeField] private int currentAmmo = 8; 

    private bool isReloading;
    private float reloadEndTime; 
    private float nextFireTime;
    private ProjectileFactory projectileFactory;
    private ProjectilePool projectilePool;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Log($"[Weapon] Awake вызван на оружии: {gameObject.name}");
    }

    private void Start()
    {
        currentAmmo = maxAmmo;
        Debug.Log($"[Weapon] Start: Оружие '{weaponName}' инициализировано. Патронов: {currentAmmo}/{maxAmmo}");

        if (bulletPrefab == null)
            Debug.LogError($"[Weapon] ОШИБКА: На оружии {gameObject.name} не назначен BulletPrefab!");
        
        if (firePoint == null)
            Debug.LogError($"[Weapon] ОШИБКА: На оружии {gameObject.name} не назначен FirePoint!");

        // Пытаемся создать фабрику сразу
        TryInitFactory();
    }

    private void Update()
    {
        // Безопасный таймер перезарядки (без корутин)
        if (isReloading && Time.time >= reloadEndTime)
        {
            currentAmmo = maxAmmo;
            isReloading = false;
            Debug.Log($"[Weapon] Перезарядка завершена! Патроны восстановлены: {currentAmmo}/{maxAmmo}");
        }

        // Переворачиваем спрайт оружия, когда оно смотрит влево
        if (spriteRenderer == null) return;

        float angle = transform.eulerAngles.z;
        if (angle > 180f) angle -= 360f;
        bool isAimingLeft = angle > 90f || angle < -90f;
        spriteRenderer.flipY = isAimingLeft;
    }

    public void Attack()
    {
        Debug.Log($"[Weapon] Метод Attack() запущен. Текущие патроны: {currentAmmo}/{maxAmmo}, isReloading: {isReloading}");

        if (isReloading)
        {
            Debug.LogWarning("[Weapon] Атака отменена: оружие находится в процессе перезарядки!");
            return;
        }

        if (Time.time < nextFireTime)
        {
            Debug.Log($"[Weapon] Атака отменена: слишком высокая скорострельность. Ждать еще: {nextFireTime - Time.time:F2} сек.");
            return;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log("[Weapon] Магазин пуст! Вызываем метод Reload().");
            Reload();
            return;
        }

        currentAmmo--;
        nextFireTime = Time.time + fireRate;
        Debug.Log($"[Weapon] Выстрел совершен! Потрачен патрон. Осталось: {currentAmmo}/{maxAmmo}");

        switch (weaponType)
        {
            case WeaponType.Pistol:
            case WeaponType.Rifle:
            case WeaponType.Shotgun:
            case WeaponType.MachineGun:
                FireProjectile();
                break;
            case WeaponType.Laser:
                FireLaser();
                break;
            case WeaponType.Shock:
                FireShock();
                break;
            case WeaponType.Melee:
                FireMelee();
                break;
        }
    }

    public void Reload()
    {
        if (isReloading || currentAmmo == maxAmmo)
        {
            Debug.Log($"[Weapon] Перезарядка пропущена. isReloading: {isReloading}, патроны полный магазин: {currentAmmo == maxAmmo}");
            return;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        isReloading = true;
        reloadEndTime = Time.time + reloadTime;
        Debug.Log($"[Weapon] Перезарядка началась для {weaponName}. Ждем {reloadTime} сек...");
    }

    private void TryInitFactory()
    {
        if (projectileFactory == null && bulletPrefab != null)
        {
            projectilePool = new ProjectilePool(bulletPrefab);
            projectileFactory = new ProjectileFactory(bulletPrefab, bulletSpeed, projectilePool);
            Debug.Log($"[Weapon] Пул и фабрика снарядов успешно созданы.");
        }
    }

    private void FireProjectile()
    {
        Debug.Log("[Weapon] Срабатывает FireProjectile()...");

        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogError($"[Weapon] Невозможно выстрелить: BulletPrefab ({bulletPrefab}) или FirePoint ({firePoint}) не назначены!");
            return;
        }

        // Гарантируем создание фабрики (ленивая инициализация)
        TryInitFactory();

        if (projectileFactory == null)
        {
            Debug.LogError("[Weapon] ОШИБКА: projectileFactory равен null даже после попытки создания!");
            return;
        }

        GameObject spawnedBullet = projectileFactory.Spawn(firePoint, (int)damage);
        
        if (spawnedBullet != null)
        {
            Debug.Log($"[Weapon] Снаряд успешно заспавнен! Имя объекта: {spawnedBullet.name}, Позиция: {spawnedBullet.transform.position}");
        }
        else
        {
            Debug.LogError("[Weapon] Фабрика вернула null при попытке заспавнить пулю!");
        }
    }

    private void FireLaser()
    {
        if (firePoint == null) return;

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.right, laserRange);

        if (hit.collider != null)
        {
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            DamageSystem.Apply(damageable, (int)damage);
            Debug.Log($"{weaponType} laser hit {hit.collider.name}!");
        }
    }

    private void FireShock()
    {
        if (firePoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(firePoint.position, shockRadius, shockTargetLayer);

        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            DamageSystem.Apply(damageable, (int)damage);
        }

        Debug.Log($"{weaponType} shock hit {hits.Length} targets!");
    }

    private void FireMelee()
    {
        if (firePoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(firePoint.position, shockRadius, shockTargetLayer);

        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            DamageSystem.Apply(damageable, (int)damage);
        }

        Debug.Log($"{weaponType} melee hit {hits.Length} targets!");
    }
}