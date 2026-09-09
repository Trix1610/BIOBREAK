using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Core;

public sealed class PauseController
{
    public bool IsPaused { get; private set; }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
    }
}

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuPanel;
    private readonly PauseController pauseController = new();

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
            if (pauseController.IsPaused)
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
            
        pauseController.Resume();
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
            
        pauseController.Pause();
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
        SceneFlowService.Load(SceneNames.MainMenu, false);
    }
}