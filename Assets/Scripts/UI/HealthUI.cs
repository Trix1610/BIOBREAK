using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core;

public sealed class HealthPresenter
{
    public string Format(float currentHealth, float maxHealth)
    {
        return $"HP: {currentHealth:0} / {maxHealth:0}";
    }
}

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text hpText;

    private CharacterStats stats;
    private Coroutine findRoutine;
    private readonly HealthPresenter presenter = new();

    private void Start()
    {
        Debug.Log("[HealthUI] Start вызван. Начинаем поиск игрока...");
        findRoutine = StartCoroutine(FindPlayerRoutine());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (stats != null)
        {
            TrySubscribe();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Unsubscribe();
        stats = null;

        if (findRoutine != null)
            StopCoroutine(findRoutine);

        findRoutine = StartCoroutine(FindPlayerRoutine());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Unsubscribe();

        if (findRoutine != null)
        {
            StopCoroutine(findRoutine);
            findRoutine = null;
        }
    }

    private void Unsubscribe()
    {
        if (stats != null)
            stats.OnHealthChanged -= UpdateHealthDisplay;
    }

    private IEnumerator FindPlayerRoutine()
    {
        GameObject player = null;

        while (player == null || stats == null)
        {
            player = PlayerReference.Instance?.Player;

            if (player != null)
            {
                stats = player.GetComponent<CharacterStats>();
            }

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

        hpText.text = presenter.Format(currentHealth, stats.MaxHealth);
        Debug.Log($"[HealthUI] Текст на UI успешно обновлен на: {hpText.text}");
    }
}