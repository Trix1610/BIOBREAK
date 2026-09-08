using Room;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public static class SceneNames
    {
        public const string Game = "GAME";
        public const string MainMenu = "MainMenu";
        public const string StartRoom = "ROOM_00";
    }

    public static class SceneFlowService
    {
        public static bool CanUseTransition => ScreenTransition.Instance != null;

        public static bool Load(string sceneName, bool useTransition)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;

            if (useTransition && ScreenTransition.Instance != null)
            {
                ScreenTransition.Instance.LoadSceneWithTransition(sceneName);
                return true;
            }

            SceneManager.LoadScene(sceneName);
            return true;
        }

        public static bool LoadWithTransition(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName) || !CanUseTransition)
                return false;

            ScreenTransition.Instance.LoadSceneWithTransition(sceneName);
            return true;
        }
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Spawner Settings")]
        [SerializeField] private GameObject enemyPrefab; // Префаб врага
        [SerializeField] private LayerMask groundLayer;   // Слой земли/платформ
        [SerializeField] private float spawnYOffset = 1f; // Высота спавна над землей

        [Header("Reward Settings")]
        [SerializeField] private GameObject[] rewardPrefabs;   // Массив префабов наград
        [SerializeField] private float rewardHeightOffset = 1f; // Высота над центром Ground_main

        private RoomEnemySpawner _enemySpawner;
        private RoomRewardService _rewardService;
        private RoomController _roomController;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _enemySpawner = new RoomEnemySpawner(enemyPrefab, groundLayer, spawnYOffset);
            _rewardService = new RoomRewardService(rewardPrefabs, rewardHeightOffset);
            _roomController = new RoomController(_enemySpawner, _rewardService);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _roomController.Reset();

            // Гарантируем, что время всегда идет при загрузке новой сцены
            Time.timeScale = 1f;

            if (scene.name == SceneNames.StartRoom || scene.name == SceneNames.Game)
            {
                return;
            }

            // Проверяем, зачищена ли комната при входе
            if (RunManager.Instance != null && RunManager.Instance.IsCurrentRoomCleared())
            {
                string currentRoom = scene.name;
                if (!RunManager.Instance.IsRewardCollected(currentRoom))
                {
                    _roomController.MarkRewardSpawned();
                    SpawnRewardAboveGroundMain();
                }
                else
                {
                    _roomController.MarkRewardSpawned();
                }
            }
        }

        // Публичный метод для вызова из триггера двери
        public bool TriggerRoomActivation()
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == SceneNames.StartRoom || currentScene == SceneNames.Game)
                return false;

            // Если враги уже спавнились или комната уже зачищена — ничего не делаем
            if (_roomController.IsActive)
                return true;

            if (RunManager.Instance != null && RunManager.Instance.IsCurrentRoomCleared())
                return false;

            // Спавним врагов мгновенно (без задержек)
            return _roomController.TryActivate();
        }

        private void Update()
        {
            string currentScene = SceneManager.GetActiveScene().name;
    
            if (currentScene == SceneNames.StartRoom || currentScene == SceneNames.Game || _roomController.RewardSpawned || !_roomController.IsActive)
                return;

            if (RunManager.Instance != null && RunManager.Instance.IsCurrentRoomCleared())
            {
                _roomController.MarkRewardSpawned();
                
                if (!RunManager.Instance.IsRewardCollected(currentScene))
                {
                    RunManager.Instance.MarkRewardAsSpawned(currentScene);
                    SpawnRewardAboveGroundMain();
                }
                return;
            }

            // Если враги заспавнены и их больше не осталось на сцене — комната зачищена!
            if (AreEnemiesCleared())
            {
                _roomController.MarkRewardSpawned();

                if (RunManager.Instance != null)
                {
                    RunManager.Instance.MarkCurrentRoomAsCleared();
                    RunManager.Instance.MarkRewardAsSpawned(currentScene);
                }

                SpawnRewardAboveGroundMain();
            }
        }

        private void SpawnRewardAboveGroundMain()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            _roomController.SpawnReward(currentScene);
        }

        public bool AreEnemiesCleared()
        {
            if (RunManager.Instance != null && RunManager.Instance.IsCurrentRoomCleared())
                return true;

            if (!_roomController.IsActive)
                return false;

            return _roomController.AreEnemiesCleared();
        }

        public bool IsRoomActive => _roomController.IsActive;

        public void NotifyEnemyDefeated()
        {
            _roomController.NotifyEnemyDefeated();
        }
    }
}