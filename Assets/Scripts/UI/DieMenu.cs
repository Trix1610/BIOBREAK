using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class DieMenu : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private GameObject dieMenuUI; // Ссылка на панель DieMenu
    
    private CharacterStats playerStats;

    private void Start()
    {
        // Запускаем поиск игрока (на случай, если он спавнится с задержкой)
        StartCoroutine(FindPlayerAndSubscribe());

        if (dieMenuUI != null)
            dieMenuUI.SetActive(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UnsubscribeFromPlayer();
        StartCoroutine(FindPlayerAndSubscribe());
    }

    private System.Collections.IEnumerator FindPlayerAndSubscribe()
    {
        // Ждем, пока игрок появится на сцене и у него появится компонент CharacterStats
        GameObject player = null;
        while (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerStats = player.GetComponent<CharacterStats>();
                if (playerStats != null)
                {
                    playerStats.OnDeath += ShowDieMenu;
                    Debug.Log("DieMenu успешно подписался на событие смерти игрока!");
                    yield break; // Выходим из корутины, всё нашли
                }
            }
            yield return null; // Ждем следующий кадр
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromPlayer();
    }

    private void UnsubscribeFromPlayer()
    {
        if (playerStats != null)
        {
            playerStats.OnDeath -= ShowDieMenu;
            playerStats = null;
        }
    }
    
    public void ShowDieMenu()
    {
        Debug.Log(">>> МЕТОД SHOW DIEMENU ВЫЗВАН! <<<"); // Посмотрим, появится ли это в консоли

        if (dieMenuUI != null)
        {
            dieMenuUI.SetActive(true);
            Debug.Log(">>> Панель dieMenuUI успешно активирована (SetActive(true))! <<<");
        }
        else
        {
            Debug.LogError(">>> ОШИБКА: Поле dieMenuUI не заполнено в инспекторе! <<<");
        }
    
        Time.timeScale = 0f; 
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        // 1. Получаем фиктивную сцену DontDestroyOnLoad через один из объектов
        GameObject dummy = new GameObject("Temp");
        Object.DontDestroyOnLoad(dummy);
        Scene ddolScene = dummy.scene;
        Destroy(dummy); // Удаляем временный объект

        // 2. Достаем все корневые объекты из этой сцены и уничтожаем их
        GameObject[] rootObjects = ddolScene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            Destroy(obj);
        }

        SceneManager.LoadScene("GAME");
    }
    
    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        // 1. Получаем фиктивную сцену DontDestroyOnLoad через один из объектов
        GameObject dummy = new GameObject("Temp");
        Object.DontDestroyOnLoad(dummy);
        Scene ddolScene = dummy.scene;
        Destroy(dummy); // Удаляем временный объект

        // 2. Достаем все корневые объекты из этой сцены и уничтожаем их
        GameObject[] rootObjects = ddolScene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            Destroy(obj);
        }
        SceneManager.LoadScene("MainMenu");
    }
}
