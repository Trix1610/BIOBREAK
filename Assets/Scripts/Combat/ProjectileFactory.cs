using UnityEngine;
using Weapons;

namespace Weapons
{
    public sealed class ProjectileFactory
    {
        private readonly ProjectilePool pool;
        private readonly float speed;

        public ProjectileFactory(GameObject projectilePrefab, float speed, ProjectilePool pool)
        {
            this.pool = pool;
            this.speed = speed;
        }

        public bool Spawn(Transform firePoint, int damage)
        {
            if (pool == null || firePoint == null)
                return false;

            GameObject projectile = pool.Get(firePoint.position, firePoint.rotation);

            if (projectile == null)
                return false;

            Bullet bullet = projectile.GetComponent<Bullet>();
            bullet?.Initialize(damage, pool.Release);

            Rigidbody2D body = projectile.GetComponent<Rigidbody2D>();
            if (body != null)
                body.linearVelocity = firePoint.right * speed;

            return true;
        }
    }
}
