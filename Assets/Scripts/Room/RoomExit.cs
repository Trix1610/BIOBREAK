using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Room
{
    public class RoomExit : MonoBehaviour
    {
        [Header("Exit")]
        [SerializeField] private string direction;

        [Header("Gate Visual & Physics")]
        [SerializeField] private Image gateImage;                  // Ссылка на ваш UI Image с Filled режимом
        [SerializeField] private Collider2D gateCollider;           // Физическая преграда
        [SerializeField] private float fadeDuration = 0.5f;        // Время исчезновения / появления

        private bool _isUsed;
        private bool _isGateOpened = false;

        private IEnumerator Start()
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == "ROOM_00" || currentScene == "GAME")
            {
                OpenGateInstant();
                yield break;
            }

            if (gateCollider != null)
                gateCollider.enabled = false;

            // Изначально скрываем ворота, чтобы проиграть красивое появление сверху вниз
            if (gateImage != null)
            {
                gateImage.enabled = false;
                gateImage.fillAmount = 0f;
            }

            yield return new WaitForSeconds(0.5f); 

            CheckGateStatusSmooth();
        }

        private void Update()
        {
            if (_isGateOpened)
                return;

            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == "ROOM_00" || currentScene == "GAME")
                return;

            if (GameManager.Instance != null && GameManager.Instance.AreEnemiesCleared())
            {
                OpenGateSmooth();
            }
        }

        private void CheckGateStatusSmooth()
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == "ROOM_00" || currentScene == "GAME")
            {
                OpenGateInstant();
                return;
            }

            if (GameManager.Instance != null &&
                !GameManager.Instance.IsRoomActive &&
                (RunManager.Instance == null || !RunManager.Instance.IsCurrentRoomCleared()))
            {
                OpenGateInstant();
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.AreEnemiesCleared())
            {
                OpenGateInstant();
            }
            else
            {
                // Если враги не убиты, ворота должны появиться сверху вниз
                CloseGateSmooth();
            }
        }

        private void CloseGateInstant()
        {
            _isGateOpened = false;

            if (gateCollider != null) 
                gateCollider.enabled = true;

            if (gateImage != null) 
            {
                gateImage.enabled = true;
                gateImage.fillAmount = 1f; 
            }
        }

        public void CloseGateSmooth()
        {
            _isGateOpened = false;

            if (gateCollider != null) 
                gateCollider.enabled = true;

            if (gateImage != null) 
            {
                StartCoroutine(ExpandGateFillRoutine());
            }
        }

        private void OpenGateInstant()
        {
            _isGateOpened = true;

            if (gateCollider != null) 
                gateCollider.enabled = false;

            if (gateImage != null) 
            {
                gateImage.enabled = false;
                gateImage.fillAmount = 0f;
            }
        }

        private void OpenGateSmooth()
        {
            _isGateOpened = true;

            if (gateCollider != null)
            {
                gateCollider.enabled = false;
            }

            if (gateImage != null)
            {
                StartCoroutine(ShrinkGateFillRoutine());
            }
        }

        // Плавное исчезновение (снизу вверх)
        private IEnumerator ShrinkGateFillRoutine()
        {
            float elapsedTime = 0f;
            float startFill = gateImage.fillAmount;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;

                gateImage.fillAmount = Mathf.Lerp(startFill, 0f, t);

                yield return null;
            }

            gateImage.enabled = false;
            gateImage.fillAmount = 1f;
        }

        // Плавное появление (сверху вниз)
        private IEnumerator ExpandGateFillRoutine()
        {
            gateImage.enabled = true;
            gateImage.fillAmount = 0f; // Начинаем с нуля (пусто)

            float elapsedTime = 0f;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;

                // Заполняем от 0 до 1 (при Vertical + Bottom это визуально растет сверху вниз)
                gateImage.fillAmount = Mathf.Lerp(0f, 1f, t);

                yield return null;
            }

            gateImage.fillAmount = 1f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (_isUsed)
                return;

            string currentRoom = SceneManager.GetActiveScene().name;
            if (currentRoom == "ROOM_00" || currentRoom == "GAME")
            {
                OpenGateInstant();
            }

            if (currentRoom != "ROOM_00" && currentRoom != "GAME" &&
                GameManager.Instance != null && !GameManager.Instance.AreEnemiesCleared())
            {
                Debug.Log("Дверь заблокирована! Сначала уничтожьте всех врагов.");
                return;
            }

            if (RunManager.Instance == null)
            {
                Debug.LogError("RoomExit: RunManager.Instance is NULL.");
                return;
            }

            string destinationRoom = RunManager.Instance.GetDestination(currentRoom, direction);

            if (string.IsNullOrEmpty(destinationRoom))
            {
                Debug.LogError($"RoomExit: destination not found for {currentRoom} -> {direction}");
                return;
            }

            string destinationSpawn = (direction == "Right") ? "Spawn_Left" : "Spawn_Right";

            if (ScreenTransition.Instance == null)
            {
                Debug.LogError("RoomExit: ScreenTransition.Instance is NULL.");
                return;
            }

            _isUsed = true;
            CharacterSpawnData.SetSpawn(destinationSpawn);
            ScreenTransition.Instance.LoadSceneWithTransition(destinationRoom);
        }
    }
}