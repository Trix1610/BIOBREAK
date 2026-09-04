using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f; // Время жизни пули, чтобы она не летела вечно

    private void Start()
    {
        // Уничтожаем пулю через N секунд, если она ни во что не попала
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Пытаемся получить компонент Enemy с объекта, в который врезались
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(1); // Наносим урон
        }

        // Уничтожаем пулю при любом столкновении (со стеной или врагом)
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Срабатывает, если Is Trigger ВКЛЮЧЕН
        
        // Уничтожаем пулю при прохождении сквозь триггер (если нужно)
        Destroy(gameObject);
    }
}