using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text hpText;

    private CharacterStats stats;

    private void Start()
    {
        FindPlayer();
    }

    private void OnEnable()
    {
        if (stats != null)
        {
            stats.OnHealthChanged += UpdateHealthDisplay;
            UpdateHealthDisplay(stats.CurrentHealth);
        }
    }

    private void OnDisable()
    {
        if (stats != null)
        {
            stats.OnHealthChanged -= UpdateHealthDisplay;
        }
    }

    private void FindPlayer()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag("Player");

        if (player == null)
            return;

        stats = player.GetComponent<CharacterStats>();

        if (stats == null)
        {
            Debug.LogError(
                "HealthUI: CharacterStats not found on Player."
            );
        }
        else
        {
            stats.OnHealthChanged += UpdateHealthDisplay;
            UpdateHealthDisplay(stats.CurrentHealth);
        }
    }

    private void UpdateHealthDisplay(float currentHealth)
    {
        if (hpText != null && stats != null)
        {
            hpText.text =
                $"HP: {currentHealth:0} / {stats.MaxHealth:0}";
        }
    }
}