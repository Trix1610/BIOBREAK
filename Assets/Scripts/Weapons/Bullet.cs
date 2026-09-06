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
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage); // Наносим переданный урон
            }

            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Если это триггер комнаты (или мы можем проверять, что это НЕ игрок и НЕ враг, кому наносим урон)
            // Допустим, у нашего триггера комнаты можно проверить тег или просто игнорировать его:
            if (collision.CompareTag("RoomTrigger")) // Можно дать триггеру тег RoomTrigger
            {
                return; // Пуля просто пролетает сквозь него сквозь и не уничтожается!
            }

            // Старая логика для остальных триггеров
            Destroy(gameObject);
        }
    }
}