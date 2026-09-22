using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Core;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string firstGameScene = SceneNames.StartRoom; 
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject settingsPanel;

    private InputAction menuNavigateAction;
    private InputAction menuSubmitAction;
    private Button[] menuButtons;
    private int selectedIndex = 0;

    private void Start()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            menuNavigateAction = playerInput.actions.FindAction("MenuNavigate", throwIfNotFound: false);
            menuSubmitAction = playerInput.actions.FindAction("MenuSubmit", throwIfNotFound: false);
        }

        var buttons = new System.Collections.Generic.List<Button>();
        
        if (startButton != null)
        {
            buttons.Add(startButton);
            startButton.onClick.AddListener(OnStartGameClicked);
            AddButtonHighlight(startButton);
        }

        if (settingsButton != null)
        {
            buttons.Add(settingsButton);
            settingsButton.onClick.AddListener(OnSettingsClicked);
            AddButtonHighlight(settingsButton);
        }

        if (exitButton != null)
        {
            buttons.Add(exitButton);
            exitButton.onClick.AddListener(OnExitGameClicked);
            AddButtonHighlight(exitButton);
        }

        menuButtons = buttons.ToArray();

        Debug.Log($"[MainMenu] Кнопок в меню: {menuButtons.Length}");

        if (menuButtons.Length > 0)
            SelectButton(0);
    }

    private void AddButtonHighlight(Button button)
    {
        ColorBlock colors = button.colors;
        Color normalColor = colors.normalColor;
        Color highlightColor = new Color(1f, 0.8f, 0f); // Желтоватый
        
        colors.highlightedColor = highlightColor;
        colors.pressedColor = new Color(0.8f, 0.6f, 0f);
        button.colors = colors;
    }

    private void Update()
    {
        if (menuButtons == null || menuButtons.Length == 0)
            return;

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

    public void OnStartGameClicked()
    {
        // Добавляем отладку, чтобы сразу увидеть в консоли нажатие
        Debug.Log("Кнопка Старт нажата!");

        if (RunManager.Instance != null)
        {
            RunManager.Instance.StartNewRun();
        }

        SceneFlowService.Load(firstGameScene, false);
    }

    public void OnSettingsClicked()
    {
        Debug.Log("Кнопка Настройки нажата!");
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Debug.Log("[MainMenu] Панель настроек открыта.");
        }
        else
        {
            Debug.LogError("[MainMenu] settingsPanel не назначен в инспекторе!");
        }
    }

    public void OnSettingsBackClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Debug.Log("[MainMenu] Панель настроек закрыта.");
        }
    }

    public void OnExitGameClicked()
    {
        Debug.Log("Кнопка Выход нажата!");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
