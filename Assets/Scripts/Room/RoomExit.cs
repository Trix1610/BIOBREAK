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
        private bool _isGateOpened = true;
        private bool _isClosing = false;

        private void Start()
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == SceneNames.StartRoom || currentScene == SceneNames.Game)
            {
                OpenGateInstant();
                return;
            }

            // Если комната уже зачищена в раннере — открыто
            if (RunManager.Instance != null && RunManager.Instance.IsCurrentRoomCleared())
            {
                OpenGateInstant();
            }
            else
            {
                // По умолчанию при входе ворота ОТКРЫТЫ, пока не сработает RoomTrigger боя
                OpenGateInstant();
            }
        }

        private void Update()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == SceneNames.StartRoom || currentScene == SceneNames.Game)
                return;

            if (GameManager.Instance != null && GameManager.Instance.AreEnemiesCleared())
            {
                if (!_isGateOpened)
                    OpenGateSmooth();
            }
        }

        private void CloseGateInstant()
        {
            _isGateOpened = false;
            _isClosing = false;

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
            if (_isClosing || (!_isGateOpened && gateCollider != null && gateCollider.enabled))
                return;

            _isClosing = true;
            _isGateOpened = false;

            if (gateCollider != null) 
                gateCollider.enabled = true;

            if (gateImage != null) 
            {
                StartCoroutine(ExpandGateFillRoutine());
            }
            else
            {
                _isClosing = false;
            }
        }

        private void OpenGateInstant()
        {
            _isGateOpened = true;
            _isClosing = false;

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
            _isClosing = false;

            if (gateCollider != null)
            {
                gateCollider.enabled = false;
            }

            if (gateImage != null)
            {
                StartCoroutine(ShrinkGateFillRoutine());
            }
        }

        private IEnumerator ShrinkGateFillRoutine()
        {
            float elapsedTime = 0f;
            float startFill = gateImage != null ? gateImage.fillAmount : 1f;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;
                if (gateImage != null)
                    gateImage.fillAmount = Mathf.Lerp(startFill, 0f, t);
                yield return null;
            }

            if (gateImage != null)
            {
                gateImage.enabled = false;
                gateImage.fillAmount = 0f;
            }
        }

        private IEnumerator ExpandGateFillRoutine()
        {
            if (gateImage != null)
            {
                gateImage.enabled = true;
                gateImage.fillAmount = 0f;
            }

            float elapsedTime = 0f;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeDuration;
                if (gateImage != null)
                    gateImage.fillAmount = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            if (gateImage != null)
                gateImage.fillAmount = 1f;

            _isClosing = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (_isUsed)
                return;

            string currentRoom = SceneManager.GetActiveScene().name;

            // Блокируем ТОЛЬКО если бой реально идет (комната активна через RoomTrigger) и враги не зачищены.
            // Пока ты не наступил на триггер спавна, IsRoomActive == false, и ты свободно можешь идти назад.
            if (currentRoom != SceneNames.StartRoom && currentRoom != SceneNames.Game &&
                GameManager.Instance != null && GameManager.Instance.IsRoomActive && !GameManager.Instance.AreEnemiesCleared())
            {
                Debug.Log("Дверь заблокирована! Сначала уничтожьте всех врагов.");
                return;
            }

            if (RunManager.Instance == null)
            {
                Debug.LogError("RoomExit: RunManager.Instance is NULL.");
                return;
            }

            string destinationRoom = RunManager.Instance.GetDestination(currentroomOrCurrentScene(currentRoom), direction);
            // Исправлено обращение к переменной сцены ниже:
            string destinationRoomActual = RunManager.Instance.GetDestination(currentRoom, direction);

            if (string.IsNullOrEmpty(destinationRoomActual))
            {
                Debug.LogError($"RoomExit: destination not found for {currentRoom} -> {direction}");
                return;
            }

            string destinationSpawn = (direction == "Right") ? "Spawn_Left" : "Spawn_Right";

            if (!SceneFlowService.CanUseTransition)
            {
                Debug.LogError("RoomExit: ScreenTransition.Instance is NULL.");
                return;
            }

            _isUsed = true;
            CharacterSpawnData.SetSpawn(destinationSpawn);
            SceneFlowService.LoadWithTransition(destinationRoomActual);
        }
        
        private string currentroomOrCurrentScene(string s) => s;
    }
}