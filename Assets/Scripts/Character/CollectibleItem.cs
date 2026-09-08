using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Character
{
    // Структура для настройки одного баффа прямо в инспекторе
    [System.Serializable]
    public struct StatBuff
    {
        public StatType statType;          // Какой стат меняем (MaxHealth, MaxJumps и т.д.)
        public float value;                // Значение (например, 1 или 10)
        public StatModifierType modifierType; // Тип модификатора (Flat или Percent)
    }

    public class CollectibleItem : MonoBehaviour
    {
        [Header("Item Buffs")]
        [SerializeField] private List<StatBuff> buffs = new List<StatBuff>(); // Список всех баффов предмета
        private bool isCollected;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isCollected || !collision.CompareTag("Player"))
                return;

            CharacterStats stats =
                collision.GetComponentInParent<CharacterStats>() ??
                collision.GetComponentInChildren<CharacterStats>();

            if (stats == null)
            {
                Debug.LogWarning("На объекте игрока не найден компонент CharacterStats!");
                return;
            }

            isCollected = true;
            float healthBonusAdded = 0f;

            // Проходим по всем настроенным в инспекторе баффам и применяем их
            foreach (var buff in buffs)
            {
                StatModifier modifier = new StatModifier(buff.value, buff.modifierType, this);
                stats.AddStatModifier(buff.statType, modifier);

                // Если увеличиваем максимальное здоровье, считаем бонус
                if (buff.statType == StatType.MaxHealth)
                {
                    healthBonusAdded += buff.value;
                }
            }

            // Если был бонус к здоровью, лечим игрока на это же значение,
            // чтобы текущее здоровье поднялось до максимума и сразу обновился UI
            if (healthBonusAdded > 0f)
            {
                stats.Heal(healthBonusAdded);
            }

            Debug.Log($"Предмет подобран! Применено баффов: {buffs.Count}");

            // Фиксируем в RunManager, что награда в этой комнате собрана
            if (RunManager.Instance != null)
            {
                string currentRoom = SceneManager.GetActiveScene().name;
                RunManager.Instance.MarkRewardAsCollected(currentRoom);
            }

            // Уничтожаем предмет после подбора
            Destroy(gameObject);
        }
    }
}