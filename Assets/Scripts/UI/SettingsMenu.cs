using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button backButton;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button controlsButton;
    [SerializeField] private GameObject controlsPanel;

    private Selectable[] menuElements;
    private int selectedIndex = 0;
    private GameObject previousPanel;
    private bool isOpenThisFrame = false; // Защита от мгновенного закрытия

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
        // Собираем все элементы управления для навигации стрелочками
        menuElements = new Selectable[] { masterVolumeSlider, musicVolumeSlider, sfxVolumeSlider, fullscreenToggle, controlsButton, backButton };

        if (controlsButton != null)
        {
            controlsButton.onClick.AddListener(OnControlsClicked);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
            AddButtonHighlight(backButton);
        }

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(_ => OnMasterVolumeChanged());
            
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(_ => OnFullscreenChanged());

        LoadSettings();
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
        if (Keyboard.current == null) return;
        if (settingsPanel == null || !settingsPanel.activeSelf) return;

        if (isOpenThisFrame)
        {
            isOpenThisFrame = false;
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
            return;
        }

        // Проверяем, не нажал ли игрок стрелочки
        bool keyboardNavigated = false;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = menuElements.Length - 1;
            keyboardNavigated = true;
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            selectedIndex++;
            if (selectedIndex >= menuElements.Length) selectedIndex = 0;
            keyboardNavigated = true;
        }

        if (keyboardNavigated)
        {
            HighlightElement(selectedIndex);
        }

        // Синхронизация с мышью: если игрок кликнул мышкой по какому-то элементу, 
        // находим его индекс, чтобы при нажатии стрелочек навигация не скакала в начало
        var currentSelected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (currentSelected != null)
        {
            for (int i = 0; i < menuElements.Length; i++)
            {
                if (menuElements[i] != null && menuElements[i].gameObject == currentSelected)
                {
                    selectedIndex = i;
                    break;
                }
            }
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (menuElements[selectedIndex] is Button btn)
            {
                btn.onClick.Invoke();
            }
            else if (menuElements[selectedIndex] is Toggle tgl)
            {
                tgl.isOn = !tgl.isOn;
            }
        }
    }

    private void HighlightElement(int index)
    {
        for (int i = 0; i < menuElements.Length; i++)
        {
            if (menuElements[i] != null)
            {
                if (i == index)
                {
                    // Передаем фокус в EventSystem. Unity сама активирует визуальное выделение (Highlighted/Selected)
                    menuElements[i].Select();
                }
            }
        }
    }

    public void Open(GameObject panelToHide)
    {
        previousPanel = panelToHide;
        if (previousPanel != null)
            previousPanel.SetActive(false);
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            isOpenThisFrame = true; // Активируем защиту от закрытия в тот же кадр
            selectedIndex = 0;
            HighlightElement(selectedIndex);
            Debug.Log("[SettingsMenu] Меню настроек открыто.");
        }
    }

    public void Close()
    {
        SaveSettings();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        if (previousPanel != null)
        {
            previousPanel.SetActive(true);
        
            if (PauseMenu.Instance != null)
            {
                PauseMenu.Instance.OnCloseSettings();
            }
        }

        // Возвращаем фокус ввода в EventSystem, чтобы клавиатура снова работала
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            // Очищаем текущий выбор и даем системе понять, что интерфейс активен
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    
        Debug.Log("[SettingsMenu] Меню настроек закрыто.");
    }

    private void OnBackClicked()
    {
        Close();
    }
    public void OnControlsClicked()
    {
        if (controlsPanel != null && settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            controlsPanel.SetActive(true);
        
            // Если на панели управления есть свой скрипт, можно сообщить ему, куда возвращаться
            if (ControlsMenu.Instance != null)
            {
                ControlsMenu.Instance.SetPreviousPanel(settingsPanel);
            }
        
            Debug.Log("[SettingsMenu] Открыта панель управления.");
        }
        else
        {
            Debug.LogError("[SettingsMenu] Не задана ссылка на controlsPanel или settingsPanel в инспекторе!");
        }
    }
    private void LoadSettings()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        ApplySettings();
    }

    private void SaveSettings()
    {
        if (masterVolumeSlider != null)
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
            
        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
            
        if (sfxVolumeSlider != null)
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
            
        if (fullscreenToggle != null)
            PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
            
        PlayerPrefs.Save();
    }

    private void ApplySettings()
    {
        if (masterVolumeSlider != null)
            AudioListener.volume = masterVolumeSlider.value;
            
        if (fullscreenToggle != null)
            Screen.fullScreen = fullscreenToggle.isOn;
    }

    public void OnMasterVolumeChanged()
    {
        if (masterVolumeSlider != null)
            AudioListener.volume = masterVolumeSlider.value;
    }

    public void OnFullscreenChanged()
    {
        if (fullscreenToggle != null)
            Screen.fullScreen = fullscreenToggle.isOn;
    }
}