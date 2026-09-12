using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Core;

public class DieMenu : MonoBehaviour
{
    [Header("UI элементы")]
    [SerializeField] private GameObject dieMenuUI;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    private CharacterStats playerStats;
    private bool isMenuActive = false;
    private Button[] menuButtons;
    private int selectedIndex = 0;

    private void Start()
    {
        menuButtons = new Button[] { restartButton, mainMenuButton };

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        // Запускаем поиск игрока (на случай, если он спавнится с задержкой)
        StartCoroutine(FindPlayerAndSubscribe());

        if (dieMenuUI != null)
            dieMenuUI.SetActive(false);
    }

    private void Update()
    {
        if (!isMenuActive || Keyboard.current == null) return;

        // Навигация стрелками
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = menuButtons.Length - 1;
            SelectButton(selectedIndex);
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            selectedIndex++;
            if (selectedIndex >= menuButtons.Length) selectedIndex = 0;
            SelectButton(selectedIndex);
        }

        // Enter или Space для нажатия выбранной кнопки
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            menuButtons[selectedIndex]?.onClick.Invoke();
        }

        // R для рестарта
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }

        // Escape или M для главного меню
        if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.mKey.wasPressedThisFrame)
        {
            OnMainMenuClicked();
        }
    }

    private void SelectButton(int index)
    {
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
            {
                ColorBlock colors = menuButtons[i].colors;
                if (i == index)
                {
                    colors.normalColor = new Color(1f, 0.8f, 0f); // Желтоватый для выбранной
                }
                else
                {
                    colors.normalColor = Color.white; // Белый для остальных
                }
                menuButtons[i].colors = colors;
            }
        }
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
            isMenuActive = true;
            SelectButton(0);
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
        isMenuActive = false;
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

        SceneFlowService.Load(SceneNames.Game, false);
    }
    
    public void OnMainMenuClicked()
    {
        isMenuActive = false;
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
        SceneFlowService.Load(SceneNames.MainMenu, false);
    }
}
