using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using TMPro;

namespace Character
{
    [System.Serializable]
    public struct StatBuff
    {
        public StatType statType;
        public float value;
        public StatModifierType modifierType;
    }

    [RequireComponent(typeof(Collider2D))]
    public class CollectibleItem : MonoBehaviour
    {
        [Header("Настройки взаимодействия")]
        [SerializeField] private KeyCode legacyInteractKey = KeyCode.E;
        [SerializeField] private string playerTag = "Player";

        [Header("Отображение названия")]
        [SerializeField] private string customItemName; // Оставьте пустым, чтобы бралось имя префаба
        [SerializeField] private TextMeshPro nameLabel; // Изменено с TextMeshProUGUI на TextMeshPro (для 3D-мира)
        [SerializeField] private GameObject interactionPromptUI; 

        [Header("Item Buffs")]
        [SerializeField] private List<StatBuff> buffs = new List<StatBuff>();

        private string itemName;
        private bool isPlayerInRange;
        private bool isCollected;
        private Collider2D playerCollider;

        private void Awake()
        {
            HideUI();

            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            // Берем кастомное имя, либо очищенное имя объекта/префаба (например, "BestLungs")
            itemName = string.IsNullOrEmpty(customItemName) 
                ? gameObject.name.Replace("(Clone)", "").Trim() 
                : customItemName;

            // Если привязан TMP-компонент, сразу задаем ему текст
            if (nameLabel != null)
            {
                nameLabel.text = itemName;
            }
        }

        private void Update()
        {
            if (isCollected || !isPlayerInRange)
                return;

            if (CheckInteractPressed())
            {
                Collect();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isCollected) return;

            if (collision.CompareTag(playerTag))
            {
                isPlayerInRange = true;
                playerCollider = collision;
                ShowUI();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag(playerTag))
            {
                isPlayerInRange = false;
                playerCollider = null;
                HideUI();
            }
        }

        private void ShowUI()
        {
            if (nameLabel != null)
                nameLabel.gameObject.SetActive(true);

            if (interactionPromptUI != null)
                interactionPromptUI.SetActive(true);
        }

        private void HideUI()
        {
            if (nameLabel != null)
                nameLabel.gameObject.SetActive(false);

            if (interactionPromptUI != null)
                interactionPromptUI.SetActive(false);
        }

        private bool CheckInteractPressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (Keyboard.current != null)
            {
                var key = Key.E; 
                return Keyboard.current[key].wasPressedThisFrame;
            }
            return false;
#else
            return Input.GetKeyDown(legacyInteractKey);
#endif
        }

        private void Collect()
        {
            if (isCollected || playerCollider == null)
                return;

            CharacterStats stats =
                playerCollider.GetComponentInParent<CharacterStats>() ??
                playerCollider.GetComponentInChildren<CharacterStats>();

            if (stats == null)
            {
                Debug.LogWarning("На объекте игрока не найден компонент CharacterStats!");
                return;
            }

            isCollected = true;
            HideUI();

            float healthBonusAdded = 0f;

            foreach (var buff in buffs)
            {
                StatModifier modifier = new StatModifier(buff.value, buff.modifierType, this);
                stats.AddStatModifier(buff.statType, modifier);

                if (buff.statType == StatType.MaxHealth)
                {
                    healthBonusAdded += buff.value;
                }
            }

            if (healthBonusAdded > 0f)
            {
                stats.Heal(healthBonusAdded);
            }

            Debug.Log($"Предмет подобран: {itemName}");

            if (RunManager.Instance != null)
            {
                string currentRoom = SceneManager.GetActiveScene().name;
                RunManager.Instance.MarkRewardAsCollected(currentRoom);
            }

            Destroy(gameObject);
        }
    }
}