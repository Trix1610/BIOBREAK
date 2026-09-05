using System.Collections;
using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text hpText;

    private CharacterStats stats;
    private Coroutine findRoutine;

    private void Start()
    {
        Debug.Log("[HealthUI] Start вызван. Начинаем поиск игрока...");
        findRoutine = StartCoroutine(FindPlayerRoutine());
    }

    private void OnEnable()
    {
        Debug.Log("[HealthUI] OnEnable вызван.");
        // Если игрок уже был найден ранее, просто пробуем подписаться
        if (stats != null)
        {
            TrySubscribe();
        }
    }

    private void OnDisable()
    {
        if (stats != null)
        {
            stats.OnHealthChanged -= UpdateHealthDisplay;
        }

        // Останавливаем корутину, если объект выключился
        if (findRoutine != null)
        {
            StopCoroutine(findRoutine);
            findRoutine = null;
        }
    }

    private IEnumerator FindPlayerRoutine()
    {
        GameObject player = null;

        // Цикл будет крутиться, пока игрок или его компонент не появятся
        while (player == null || stats == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                stats = player.GetComponent<CharacterStats>();
            }

            // Если кто-то из них все еще не найден, ждем 0.2 секунды и повторяем
            if (player == null || stats == null)
            {
                yield return new WaitForSeconds(0.2f);
            }
        }

        Debug.Log("[HealthUI] Игрок и CharacterStats успешно найдены!");
        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (stats == null)
        {
            Debug.LogWarning("[HealthUI] TrySubscribe пропущен: stats == null.");
            return;
        }

        stats.OnHealthChanged -= UpdateHealthDisplay;
        stats.OnHealthChanged += UpdateHealthDisplay;
        Debug.Log("[HealthUI] Успешно подписались на событие OnHealthChanged.");

        UpdateHealthDisplay(stats.CurrentHealth);
    }

    private void UpdateHealthDisplay(float currentHealth)
    {
        Debug.Log($"[HealthUI] UpdateHealthDisplay вызван со значением HP: {currentHealth}");

        if (hpText == null)
        {
            Debug.LogError("[HealthUI] ОШИБКА: Не назначена ссылка на TMP_Text (hpText) в инспекторе!");
            return;
        }

        if (stats == null)
        {
            Debug.LogError("[HealthUI] ОШИБКА: stats == null внутри UpdateHealthDisplay!");
            return;
        }

        hpText.text = $"HP: {currentHealth:0} / {stats.MaxHealth:0}";
        Debug.Log($"[HealthUI] Текст на UI успешно обновлен на: {hpText.text}");
    }
}