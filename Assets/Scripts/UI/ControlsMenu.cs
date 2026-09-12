using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class ControlsMenu : MonoBehaviour
{
    public static ControlsMenu Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button backButton;

    [Header("Button Texts (TextMeshPro)")]
    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private TextMeshProUGUI jumpText;

    [Header("Input Actions References")]
    [SerializeField] private InputActionReference moveAction; // Действие движения (Composite)
    [SerializeField] private InputActionReference jumpAction; // Действие прыжка

    [Header("Overlay (Optional)")]
    [SerializeField] private GameObject waitingForInputOverlay; // Текст "Нажмите клавишу..."

    private Selectable[] menuElements;
    private int selectedIndex = 0;
    private GameObject previousPanel;
    private bool isOpenThisFrame = false;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

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
        // Собираем все интерактивные элементы в массив для навигации стрелочками
        menuElements = new Selectable[] { moveLeftButton, moveRightButton, jumpButton, backButton };

        // Подписываем кнопки на действия
        if (moveLeftButton != null) moveLeftButton.onClick.AddListener(() => StartRebindForMove("Left"));
        if (moveRightButton != null) moveRightButton.onClick.AddListener(() => StartRebindForMove("Right"));
        if (jumpButton != null) jumpButton.onClick.AddListener(StartRebindForJump);
        if (backButton != null) backButton.onClick.AddListener(Close);

        if (waitingForInputOverlay != null)
            waitingForInputOverlay.SetActive(false);

        UpdateUI();
    }

    private void UpdateKeysDisplay()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Обновляем текст для прыжка
        if (jumpAction != null && jumpText != null)
        {
            string jumpKey = jumpAction.action.GetBindingDisplayString();
            jumpText.text = $"Прыжок: {jumpKey}";
        }

        // Обновляем текст для движения (ищем привязки Left и Right)
        if (moveAction != null)
        {
            for (int i = 0; i < moveAction.action.bindings.Count; i++)
            {
                var binding = moveAction.action.bindings[i];
                string bindingName = moveAction.action.GetBindingDisplayString(i);

                if (binding.name == "Left" && moveLeftText != null)
                {
                    moveLeftText.text = $"Влево: {bindingName}";
                }
                else if (binding.name == "Right" && moveRightText != null)
                {
                    moveRightText.text = $"Вправо: {bindingName}";
                }
            }
        }
    }

    private void StartRebindForJump()
    {
        if (jumpAction == null) return;
        BeginRebind(jumpAction.action, jumpText, "Прыжок");
    }

    private void StartRebindForMove(string partName)
    {
        if (moveAction == null) return;

        int bindingIndex = -1;
        for (int i = 0; i < moveAction.action.bindings.Count; i++)
        {
            if (moveAction.action.bindings[i].name == partName)
            {
                bindingIndex = i;
                break;
            }
        }

        if (bindingIndex != -1)
        {
            var targetText = (partName == "Left") ? moveLeftText : moveRightText;
            BeginRebindBindingIndex(moveAction.action, bindingIndex, targetText, partName == "Left" ? "Влево" : "Вправо");
        }
    }

    private void BeginRebind(InputAction action, TextMeshProUGUI textComponent, string actionLabel)
    {
        action.Disable();

        if (waitingForInputOverlay != null) waitingForInputOverlay.SetActive(true);
        if (textComponent != null) textComponent.text = $"{actionLabel}: [...]";

        rebindingOperation = action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .OnComplete(operation =>
            {
                CleanUpRebind();
                UpdateUI();
                action.Enable();
            })
            .OnCancel(operation =>
            {
                CleanUpRebind();
                UpdateUI();
                action.Enable();
            })
            .Start();
    }

    private void BeginRebindBindingIndex(InputAction action, int bindingIndex, TextMeshProUGUI textComponent, string actionLabel)
    {
        action.Disable();

        if (waitingForInputOverlay != null) waitingForInputOverlay.SetActive(true);
        if (textComponent != null) textComponent.text = $"{actionLabel}: [...]";

        rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnComplete(operation =>
            {
                CleanUpRebind();
                UpdateUI();
                action.Enable();
            })
            .OnCancel(operation =>
            {
                CleanUpRebind();
                UpdateUI();
                action.Enable();
            })
            .Start();
    }

    private void CleanUpRebind()
    {
        rebindingOperation?.Dispose();
        rebindingOperation = null;

        if (waitingForInputOverlay != null)
            waitingForInputOverlay.SetActive(false);
    }

    private void Update()
    {
        if (rebindingOperation != null) return;

        if (Keyboard.current == null) return;
        if (controlsPanel == null || !controlsPanel.activeSelf) return;

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

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = menuElements.Length - 1;
            HighlightElement(selectedIndex);
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            selectedIndex++;
            if (selectedIndex >= menuElements.Length) selectedIndex = 0;
            HighlightElement(selectedIndex);
        }

        if ((Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame) && menuElements[selectedIndex] != null)
        {
            if (menuElements[selectedIndex] is Button btn)
            {
                btn.onClick.Invoke();
            }
        }
    }

    private void HighlightElement(int index)
    {
        if (menuElements[index] != null)
        {
            menuElements[index].Select();
        }
    }

    public void Open(GameObject panelToHide)
    {
        previousPanel = panelToHide;
        if (previousPanel != null)
            previousPanel.SetActive(false);

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
            isOpenThisFrame = true;
            selectedIndex = 0;
            HighlightElement(selectedIndex);
            UpdateKeysDisplay();
        }
    }

    public void SetPreviousPanel(GameObject panel)
    {
        previousPanel = panel;
    }

    public void Close()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        if (previousPanel != null)
        {
            previousPanel.SetActive(true);
        }
    }
}