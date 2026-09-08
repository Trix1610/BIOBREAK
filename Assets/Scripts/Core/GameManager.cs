using System.Collections.Generic;
using Enemies;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
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

        private bool _rewardSpawnedInCurrentRoom = false;
        private bool _enemiesSpawned = false; // Флаг: враги уже заспавнены в этой комнате

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
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
            _rewardSpawnedInCurrentRoom = false;
            _enemiesSpawned = false;

            // Гарантируем, что время всегда идет при загрузке новой сцены
            Time.timeScale = 1f;

            if (scene.name == "ROOM_00" || scene.name == "GAME")
            {
                return;
            }

            // Проверяем, зачищена ли комната при входе
            if (RunManager.Instance != null && RunManager.Instance.IsCurrentRoomCleared())
            {
                string currentRoom = scene.name;
                if (!RunManager.Instance.IsRewardCollected(currentRoom))
                {
                    _rewardSpawnedInCurrentRoom = true;
                    SpawnRewardAboveGroundMain();
                }
                else
                {
                    _rewardSpawnedInCurrentRoom = true;
                }
            }
        }

        // Публичный метод для вызова из триггера двери
        public void TriggerRoomActivation()
        {
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == "ROOM_00" || currentScene == "GAME")
                return;

            // Если враги уже спавнились или комната уже зачищена — ничего не делаем
            if (_enemiesSpawned)
                return;

            if (RunManager.Instance != null && RunManager.Instance.IsCurrentRoomCleared())
                return;

            // Спавним врагов мгновенно (без задержек)
            SpawnEnemiesOnGround();
            _enemiesSpawned = true;
        }

        private void Update()
        {
            string currentScene = SceneManager.GetActiveScene().name;
    
            if (currentScene == "ROOM_00" || currentScene == "GAME" || _rewardSpawnedInCurrentRoom || !_enemiesSpawned)
                return;

            if (RunManager.Instance != null && RunManager.Instance.IsCurrentRoomCleared())
            {
                _rewardSpawnedInCurrentRoom = true;
                
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
                _rewardSpawnedInCurrentRoom = true;

                if (RunManager.Instance != null)
                {
                    RunManager.Instance.MarkCurrentRoomAsCleared();
                    RunManager.Instance.MarkRewardAsSpawned(currentScene);
                }

                SpawnRewardAboveGroundMain();
            }
        }

        private void SpawnEnemiesOnGround()
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning("[GameManager] Не задан префаб врага в инспекторе GameManager!");
                return;
            }

            int enemiesCount = Random.Range(3, 6);

            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            List<Transform> validPlatforms = new List<Transform>();

            foreach (var obj in allObjects)
            {
                bool isGroundLayer = ((1 << obj.layer) & groundLayer) != 0;

                if (isGroundLayer)
                {
                    if (!obj.CompareTag("Player") && !obj.CompareTag("Enemy"))
                    {
                        validPlatforms.Add(obj.transform);
                    }
                }
            }

            if (validPlatforms.Count > 0)
            {
                int enemiesToSpawn = Mathf.Min(enemiesCount, validPlatforms.Count);

                for (int i = 0; i < enemiesToSpawn; i++)
                {
                    Transform randomPlatform = validPlatforms[Random.Range(0, validPlatforms.Count)];
                
                    float spawnY = randomPlatform.position.y + spawnYOffset;
                    float spawnX = randomPlatform.position.x;

                    Collider2D col = randomPlatform.GetComponent<Collider2D>();
                    if (col != null)
                    {
                        spawnX = Random.Range(col.bounds.min.x, col.bounds.max.x);
                        spawnY = col.bounds.max.y + spawnYOffset;
                    }

                    Vector2 spawnPosition = new Vector2(spawnX, spawnY);
                    Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                }
            }
        }

        private void SpawnRewardAboveGroundMain()
        {
            if (rewardPrefabs == null || rewardPrefabs.Length == 0)
            {
                Debug.LogWarning("[GameManager] Массив префабов наград (rewardPrefabs) пуст!");
                return;
            }

            string currentScene = SceneManager.GetActiveScene().name;
            int rewardIndex = -1;

            // 1. СТРОГАЯ ПРОВЕРКА: если для этой комнаты уже сохранен индекс награды, берем его!
            if (RunManager.Instance != null && RunManager.Instance.TryGetRoomRewardIndex(currentScene, out int savedIndex))
            {
                rewardIndex = savedIndex;
                Debug.Log($"[GameManager] Загружаем ранее выпавшую награду с индексом: {rewardIndex} для комнаты {currentScene}");
            }
            else
            {
                // 2. Если это первый раз, выбираем случайно и СРАЗУ сохраняем в RunManager
                rewardIndex = Random.Range(0, rewardPrefabs.Length);
                
                if (RunManager.Instance != null)
                {
                    RunManager.Instance.SaveRoomRewardIndex(currentScene, rewardIndex);
                    Debug.Log($"[GameManager] Сгенерирована новая награда с индексом: {rewardIndex} для комнаты {currentScene}");
                }
            }

            // Находим позицию над Ground_Main для спавна
            GameObject groundMain = GameObject.Find("Ground_Main");
            Vector3 spawnPosition = Vector3.zero;

            if (groundMain != null)
            {
                Collider2D col = groundMain.GetComponent<Collider2D>();
                if (col != null)
                {
                    spawnPosition = col.bounds.center;
                }
                else
                {
                    SpriteRenderer sr = groundMain.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        spawnPosition = sr.bounds.center;
                    }
                    else
                    {
                        spawnPosition = groundMain.transform.position;
                    }
                }

                spawnPosition.y += rewardHeightOffset;
            }

            GameObject selectedPrefab = rewardPrefabs[rewardIndex];

            if (selectedPrefab != null)
            {
                Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            }
        }

        public bool AreEnemiesCleared()
        {
            Enemy[] remainingEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude);
            return remainingEnemies.Length == 0;
        }
    }
}