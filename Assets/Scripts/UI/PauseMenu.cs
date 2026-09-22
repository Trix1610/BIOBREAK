using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;

    private readonly PauseController pauseController = new();
    private InputAction pauseAction;
    private InputAction menuNavigateAction;
    private InputAction menuSubmitAction;
    private Button[] menuButtons;
    private int selectedIndex = 0;
    
    // Флаг, чтобы понимать, что мы ушли в подменю (настройки) и игра должна оставаться на паузе
    public bool IsInSubMenu { get; set; } = false;

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

    private void Start()
    {
        var buttons = new System.Collections.Generic.List<Button>();
        
        if (resumeButton != null)
        {
            buttons.Add(resumeButton);
            resumeButton.onClick.AddListener(OnResumeClicked);
            AddButtonHighlight(resumeButton);
        }

        if (settingsButton != null)
        {
            buttons.Add(settingsButton);
            settingsButton.onClick.AddListener(OnSettingsClicked);
            AddButtonHighlight(settingsButton);
        }

        if (mainMenuButton != null)
        {
            buttons.Add(mainMenuButton);
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            AddButtonHighlight(mainMenuButton);
        }

        menuButtons = buttons.ToArray();
    }

    private void AddButtonHighlight(Button button)
    {
        ColorBlock colors = button.colors;
        Color highlightColor = new Color(1f, 0.8f, 0f);
        
        colors.highlightedColor = highlightColor;
        colors.pressedColor = new Color(0.8f, 0.6f, 0f);
        button.colors = colors;
    }

    private void Update()
    {
        // Если открыты настройки, паузу по ESC обрабатывает само меню настроек
        if (IsInSubMenu) return;

        TryGetInputActions();
        if (pauseAction != null && pauseAction.WasPressedThisFrame())
        {
            Debug.Log("[PauseMenu] Получено действие Pause.");
            if (pauseController.IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        if (!pauseController.IsPaused) return;

        Vector2 navigation = menuNavigateAction != null
            ? menuNavigateAction.ReadValue<Vector2>()
            : Vector2.zero;

        if (menuNavigateAction != null &&
            menuNavigateAction.WasPressedThisFrame() && navigation.y > 0.5f)
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = menuButtons.Length - 1;
            SelectButton(selectedIndex);
        }

        if (menuNavigateAction != null &&
            menuNavigateAction.WasPressedThisFrame() && navigation.y < -0.5f)
        {
            selectedIndex++;
            if (selectedIndex >= menuButtons.Length) selectedIndex = 0;
            SelectButton(selectedIndex);
        }

        if (menuSubmitAction != null && menuSubmitAction.WasPressedThisFrame())
        {
            menuButtons[selectedIndex]?.onClick.Invoke();
        }
    }

    private void TryGetInputActions()
    {
        if (pauseAction != null && menuNavigateAction != null && menuSubmitAction != null)
            return;

        PlayerInput playerInput = FindAnyObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            pauseAction = playerInput.actions.FindAction("Pause", throwIfNotFound: false);
            menuNavigateAction = playerInput.actions.FindAction("MenuNavigate", throwIfNotFound: false);
            menuSubmitAction = playerInput.actions.FindAction("MenuSubmit", throwIfNotFound: false);
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
                    colors.normalColor = new Color(1f, 0.8f, 0f);
                }
                else
                {
                    colors.normalColor = Color.white;
                }
                menuButtons[i].colors = colors;
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
            SelectButton(0);
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

    public void OnSettingsClicked()
    {
        Debug.Log("[PauseMenu] Нажата кнопка 'Настройки'.");

        if (SettingsMenu.Instance != null)
        {
            // Устанавливаем флаг, что мы ушли в подменю, чтобы игра не снималась с паузы
            IsInSubMenu = true;

            // Передаем панель паузы в меню настроек
            SettingsMenu.Instance.Open(pauseMenuPanel);
        }
        else
        {
            Debug.LogError("[PauseMenu] Экземпляр SettingsMenu не найден на сцене!");
        }
    }

    // Метод, который SettingsMenu сможет вызвать при закрытии настроек, чтобы вернуть управление паузе
    public void OnCloseSettings()
    {
        IsInSubMenu = false;
    }

    public void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        GameObject dummy = new GameObject("Temp");
        Object.DontDestroyOnLoad(dummy);
        Scene ddolScene = dummy.scene;
        Destroy(dummy);

        GameObject[] rootObjects = ddolScene.GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            Destroy(obj);
        }
        SceneFlowService.Load(SceneNames.MainMenu, false);
    }
}
