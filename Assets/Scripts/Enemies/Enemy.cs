using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 3f;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    private Transform playerTransform;
    private Rigidbody2D rb;

    private void Awake()
    {
        // МГНОВЕННО при загрузке сцены (до появления на экране) проверяем зачистку
        if (RunManager.Instance != null && RunManager.Instance.IsCurrentRoomCleared())
        {
            Destroy(gameObject); // Уничтожаем до отрисовки первого кадра
            return;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(directionX * speed, rb.linearVelocity.y);

        if (directionX > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (directionX < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Враг получил урон! Осталось здоровья: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Враг уничтожен!");

        if (RunManager.Instance != null)
        {
            RunManager.Instance.MarkCurrentRoomAsCleared();
        }

        Destroy(gameObject);
    }
}