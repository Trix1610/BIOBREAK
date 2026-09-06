using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Room;

namespace Core
{
    public class RoomTrigger : MonoBehaviour
    {
        private bool _triggered = false;
        private float _spawnTime;

        private void Start()
        {
            // Запоминаем время появления объекта на сцене
            _spawnTime = Time.time;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Если сцена только что загрузилась (прошло меньше 0.3 сек), 
            // игнорируем любые касания, чтобы игрок не триггерил комнату при спавне
            if (Time.time - _spawnTime < 0.3f)
                return;

            if (_triggered)
                return;

            if (!other.CompareTag("Player"))
                return;

            // Проверка на пули (если вы добавили тег ранее)
            if (other.CompareTag("Bullet"))
                return;

            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == "ROOM_00" || currentScene == "GAME")
                return;

            if (RunManager.Instance != null && RunManager.Instance.IsCurrentRoomCleared())
            {
                _triggered = true;
                return;
            }

            _triggered = true;

            // 1. Спавним врагов
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerRoomActivation();
            }

            // 2. Закрываем двери
            RoomExit[] exits = FindObjectsByType<RoomExit>(FindObjectsInactive.Include);
            foreach (var exit in exits)
            {
                exit.CloseGateSmooth();
            }

            Debug.Log("[RoomTrigger] Игрок зашел в комнату: враги заспавнены, двери закрываются!");
        }
    }
}