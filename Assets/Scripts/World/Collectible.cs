using UnityEngine;

public class Collectible : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterStats stats = other.GetComponent<CharacterStats>();

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