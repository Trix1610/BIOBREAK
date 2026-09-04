using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text hpText;

    private CharacterStats stats;

    private void Update()
    {
        if (stats == null)
        {
            FindPlayer();
            return;
        }

        hpText.text =
            $"HP: {stats.CurrentHealth:0} / {stats.MaxHealth:0}";
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
    }
}
