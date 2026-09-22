using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;
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
        [SerializeField] private string playerTag = "Player";

        [Header("Отображение названия")]
        [SerializeField] private string customItemName; // Оставьте пустым, чтобы бралось имя префаба
        [SerializeField] private TextMeshPro nameLabel; // Изменено с TextMeshProUGUI на TextMeshPro (для 3D-мира)
        [SerializeField] private GameObject interactionPromptUI; 

        [Header("Item Buffs")]
        [SerializeField] private List<StatBuff> buffs = new List<StatBuff>();

        [Header("Weapon")]
        [SerializeField] private GameObject weaponPrefab; // Префаб оружия для экипировки

        private string itemName;
        private bool isPlayerInRange;
        private bool isCollected;
        private Collider2D playerCollider;
        private InputAction interactAction;

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
                PlayerInput playerInput = collision.GetComponentInParent<PlayerInput>();
                interactAction = playerInput != null
                    ? playerInput.actions.FindAction("Interact", throwIfNotFound: false)
                    : null;
                ShowUI();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag(playerTag))
            {
                isPlayerInRange = false;
                playerCollider = null;
                interactAction = null;
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
            return interactAction != null && interactAction.WasPressedThisFrame();
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

            // Экипируем оружие если указан префаб
            if (weaponPrefab != null)
            {
                CharacterController characterController =
                    playerCollider.GetComponentInParent<CharacterController>() ??
                    playerCollider.GetComponentInChildren<CharacterController>();

                if (characterController != null)
                {
                    characterController.EquipWeapon(weaponPrefab);
                }
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
