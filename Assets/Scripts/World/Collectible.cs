using UnityEngine;

public class Collectible : MonoBehaviour
{
    private bool isCollected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected || !other.CompareTag("Player"))
            return;

        isCollected = true;

        if (other.CompareTag("Player"))
        {
            CharacterStats stats =
                other.GetComponentInParent<CharacterStats>();

            if (stats != null)
            {
                StatModifier jumpModifier = new StatModifier(1, StatModifierType.Flat, this);
                stats.AddStatModifier(StatType.MaxJumps, jumpModifier);
                Debug.Log("Collectible picked up: +1 jump (Double Jump!)");
            }

            Destroy(gameObject);
        }
    }
}