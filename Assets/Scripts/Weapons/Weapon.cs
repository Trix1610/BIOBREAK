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
    }

    private void Start()
    {
        currentAmmo = maxAmmo;
        TryInitFactory();
    }

    private void Update()
    {
        if (isReloading && Time.time >= reloadEndTime)
        {
            currentAmmo = maxAmmo;
            isReloading = false;
        }

        if (spriteRenderer == null) return;

        float angle = transform.eulerAngles.z;
        if (angle > 180f) angle -= 360f;
        bool isAimingLeft = angle > 90f || angle < -90f;
        spriteRenderer.flipY = isAimingLeft;
    }

    public void Attack()
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
        nextFireTime = Time.time + fireRate;

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
            return;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        isReloading = true;
        reloadEndTime = Time.time + reloadTime;
    }

    private void TryInitFactory()
    {
        if (projectileFactory == null && bulletPrefab != null)
        {
            projectilePool = new ProjectilePool(bulletPrefab);
            projectileFactory = new ProjectileFactory(bulletPrefab, bulletSpeed, projectilePool);
        }
    }

    private void FireProjectile()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        TryInitFactory();

        if (projectileFactory == null)
            return;

        projectileFactory.Spawn(firePoint, (int)damage);
    }

    private void FireLaser()
    {
        if (firePoint == null) return;

        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, firePoint.right, laserRange);

        if (hit.collider != null)
        {
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            DamageSystem.Apply(damageable, (int)damage);
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
    }
}