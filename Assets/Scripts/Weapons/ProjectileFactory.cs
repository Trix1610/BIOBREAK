using UnityEngine;
using Weapons;

public class ProjectileFactory
{
    private readonly GameObject prefab;
    private readonly float speed;
    private readonly ProjectilePool pool;

    public ProjectileFactory(GameObject prefab, float speed, ProjectilePool pool)
    {
        this.prefab = prefab;
        this.speed = speed;
        this.pool = pool;
    }

    public GameObject Spawn(Transform firePoint, int damage)
    {
        GameObject bulletObj = pool.Get();
        
        bulletObj.transform.position = firePoint.position;
        bulletObj.transform.rotation = firePoint.rotation * Quaternion.Euler(0, 0, -90f);

        // Задаем скорость пули в направлении вращения
        Rigidbody2D rb = bulletObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 direction = firePoint.right;
            rb.linearVelocity = direction * speed;
        }

        // Передаем урон пуле
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.SetDamage(damage);
        }

        return bulletObj;
    }
}