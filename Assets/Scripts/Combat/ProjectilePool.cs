using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    public sealed class ProjectilePool
    {
        private readonly GameObject projectilePrefab;
        private readonly Stack<GameObject> available = new();

        public ProjectilePool(GameObject projectilePrefab)
        {
            this.projectilePrefab = projectilePrefab;
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject projectile = null;

            while (available.Count > 0 && projectile == null)
            {
                projectile = available.Pop();
            }

            if (projectile == null)
            {
                if (projectilePrefab == null)
                    return null;

                projectile = Object.Instantiate(projectilePrefab);
            }

            projectile.transform.SetPositionAndRotation(position, rotation);
            projectile.SetActive(true);
            return projectile;
        }

        public void Release(GameObject projectile)
        {
            if (projectile == null)
                return;

            Rigidbody2D body = projectile.GetComponent<Rigidbody2D>();
            if (body != null)
                body.linearVelocity = Vector2.zero;

            projectile.SetActive(false);
            available.Push(projectile);
        }
    }
}
