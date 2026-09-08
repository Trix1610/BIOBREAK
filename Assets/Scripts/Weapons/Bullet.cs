using Enemies;
using UnityEngine;

namespace Weapons
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float lifetime = 5f; 
        private int damage = 20; // Значение по умолчанию на случай, если не передадут

        // Метод, чтобы пистолет мог задать урон пуле при создании
        public void SetDamage(int newDamage)
        {
            damage = newDamage;
        }

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("RoomTrigger"))
                return;

            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // Наносим урон врагу
            }

            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("RoomTrigger"))
            {
                return; 
            }

            Destroy(gameObject);
        }
    }
}