using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [SerializeField] private float damage = 20f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        damageable?.TakeDamage(damage);
    }
}