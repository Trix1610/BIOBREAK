using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    private bool _isPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    
    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("[PauseMenu] Нажата клавиша ESC.");
            if (_isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        Time.timeScale = 1f;
        _isPaused = false;
        Debug.Log("[PauseMenu] Игра возобновлена.");
    }

    private void Pause()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            Debug.Log("[PauseMenu] Панель паузы активирована.");
        }
        else
        {
            Debug.LogError("[PauseMenu] Невозможно показать панель: ссылка утеряна!");
        }
            
        Time.timeScale = 0f;
        _isPaused = true;
    }

    public void OnResumeClicked()
    {
        Debug.Log("[PauseMenu] Нажата кнопка 'Продолжить'.");
        Resume();
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