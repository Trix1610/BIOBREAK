using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace UI
{
    public class ButtonHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Highlight Settings")]
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private float outlineWidth = 2f;
        
        private Outline _outline;
        private TextMeshProUGUI _text;
        private Color _originalColor;

        private void Awake()
        {
            _outline = GetComponent<Outline>();
            _text = GetComponent<TextMeshProUGUI>();
            
            if (_text != null)
            {
                _originalColor = _text.color;
            }
        }

        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (_outline == null)
            {
                _outline = gameObject.AddComponent<Outline>();
            }
            
            _outline.effectColor = highlightColor;
            _outline.effectDistance = new Vector2(outlineWidth, -outlineWidth);
            
            if (_text != null)
            {
                _text.color = highlightColor;
            }
        }

        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (_outline != null)
            {
                Destroy(_outline);
                _outline = null;
            }
            
            if (_text != null)
            {
                _text.color = _originalColor;
            }
        }
    }
}
