using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class ButtonStyler : MonoBehaviour
    {
        [Header("Style Settings")]
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] private Color highlightedColor = new Color(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color pressedColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color textColor = Color.white;
        
        [Header("Size Settings")]
        [SerializeField] private Vector2 buttonSize = new Vector2(200, 50);
        [SerializeField] private bool enforceSize = true;
        [SerializeField] private bool enableTextWrapping = false;
        
        [Header("Animation")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float animationSpeed = 5f;
        
        [Header("Shadow")]
        [SerializeField] private bool addShadow = true;
        [SerializeField] private Vector2 shadowDistance = new Vector2(2, -2);
        [SerializeField] private Color shadowColor = new Color(0, 0, 0, 0.5f);

        private Button button;
        private Image image;
        private TextMeshProUGUI text;
        private Shadow shadow;
        private RectTransform rectTransform;
        private RectTransform textRectTransform;
        private Vector3 originalScale;

        private void Awake()
        {
            button = GetComponent<Button>();
            image = GetComponent<Image>();
            text = GetComponentInChildren<TextMeshProUGUI>();
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;

            if (text != null)
            {
                textRectTransform = text.GetComponent<RectTransform>();
            }

            ApplyStyle();
        }

        private void ApplyStyle()
        {
            if (image != null)
            {
                image.color = normalColor;
            }

            if (text != null)
            {
                text.color = textColor;
                text.alignment = TextAlignmentOptions.Center;
                text.verticalAlignment = VerticalAlignmentOptions.Middle;
                text.enableAutoSizing = false;
                text.overflowMode = enableTextWrapping ? TextOverflowModes.Overflow : TextOverflowModes.Truncate;
                
                if (textRectTransform != null)
                {
                    textRectTransform.anchorMin = Vector2.zero;
                    textRectTransform.anchorMax = Vector2.one;
                    textRectTransform.offsetMin = Vector2.zero;
                    textRectTransform.offsetMax = Vector2.zero;
                }
            }

            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = normalColor;
                colors.highlightedColor = highlightedColor;
                colors.pressedColor = pressedColor;
                colors.selectedColor = highlightedColor;
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.1f;
                button.colors = colors;
            }

            if (enforceSize && rectTransform != null)
            {
                // Сбрасываем anchors чтобы они не мешали изменению размера
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = buttonSize;
            }

            if (addShadow)
            {
                shadow = gameObject.AddComponent<Shadow>();
                shadow.effectDistance = shadowDistance;
                shadow.effectColor = shadowColor;
            }
        }

        private void Update()
        {
            if (button == null || rectTransform == null) return;

            // Принудительно устанавливаем размер каждый кадр если включено
            if (enforceSize)
            {
                rectTransform.sizeDelta = buttonSize;
            }

            Vector3 targetScale = originalScale;
            
            if (button.IsHighlighted() || button.IsPressed())
            {
                targetScale = originalScale * hoverScale;
            }

            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale,
                targetScale,
                Time.deltaTime * animationSpeed
            );
        }
    }
}
