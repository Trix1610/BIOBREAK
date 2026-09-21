using System;
using System.Collections;
using UnityEngine;

namespace Weapons
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float lifetime = 5f; 
        private int damage = 20; // Значение по умолчанию на случай, если не передадут
        private Action<GameObject> release;
        private bool returned;

        // Метод, чтобы пистолет мог задать урон пуле при создании
        public void SetDamage(int newDamage)
        {
            damage = newDamage;
        }

        public void Initialize(int newDamage, Action<GameObject> releaseAction)
        {
            damage = newDamage;
            release = releaseAction;
            returned = false;
        }

        private void OnEnable()
        {
            returned = false;
            StartCoroutine(ExpireRoutine());
        }

        private IEnumerator ExpireRoutine()
        {
            yield return new WaitForSeconds(lifetime);
            ReturnToPool();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("RoomTrigger"))
                return;

            if (collision.gameObject.CompareTag("Player"))
                return;

            IDamageable damageable =
                collision.gameObject.GetComponentInParent<IDamageable>();

            DamageSystem.Apply(damageable, damage);

            ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("RoomTrigger"))
                return;

            if (collision.gameObject.CompareTag("Player"))
                return;

            IDamageable damageable =
                collision.gameObject.GetComponentInParent<IDamageable>();

            DamageSystem.Apply(damageable, damage);

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (returned)
                return;

            returned = true;

            if (release != null)
                release(gameObject);
            else
                Destroy(gameObject);
        }
    }
}