using System.Collections;
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
        [SerializeField] private GameObject[] rewardPrefabs;   // Массив префабов наград (случайный выбор)
        [SerializeField] private float rewardHeightOffset = 1f; // Высота над центром Ground_main

        private bool _rewardSpawnedInCurrentRoom = false;
        private bool _enemiesSpawned = false; // Флаг: враги уже заспавнены на сцене

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

            if (scene.name == "ROOM_00" || scene.name == "GAME")
            {
                return;
            }

            // Проверяем состояние комнаты при входе
            if (RunManager.Instance != null)
            {
                string currentRoom = scene.name;

                // Если комната уже зачищена
                if (RunManager.Instance.IsCurrentRoomCleared())
                {
                    // Если награда ЕЩЕ НЕ СОБРАНА игроком, спавним её сразу при входе!
                    if (!RunManager.Instance.IsRewardCollected(currentRoom))
                    {
                        _rewardSpawnedInCurrentRoom = true;
                        StartCoroutine(SpawnRewardDelayed());
                    }
                    else
                    {
                        _rewardSpawnedInCurrentRoom = true;
                    }
                    return; // Врагов спавнить не нужно
                }
            }

            StartCoroutine(SpawnEnemiesDelayed());
        }

        private IEnumerator SpawnRewardDelayed()
        {
            yield return new WaitForSeconds(0.1f);
            SpawnRewardAboveGroundMain();
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

        private IEnumerator SpawnEnemiesDelayed()
        {
            yield return new WaitForSeconds(0.2f);
            SpawnEnemiesOnGround();
            _enemiesSpawned = true;
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
            // Проверяем, что массив не пустой
            if (rewardPrefabs == null || rewardPrefabs.Length == 0)
            {
                Debug.LogWarning("[GameManager] Массив префабов наград (rewardPrefabs) пуст в инспекторе GameManager!");
                return;
            }

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
            else
            {
                Debug.LogWarning("[GameManager] Объект Ground_main не найден на сцене! Награда спавнится в точке (0,0).");
            }

            // Рандомно выбираем один предмет из массива
            int randomIndex = Random.Range(0, rewardPrefabs.Length);
            GameObject selectedPrefab = rewardPrefabs[randomIndex];

            if (selectedPrefab != null)
            {
                Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
                Debug.Log($"[GameManager] Комната зачищена! Выпал случайный предмет: {selectedPrefab.name}");
            }
            else
            {
                Debug.LogWarning($"[GameManager] Префаб под индексом {randomIndex} в массиве rewardPrefabs оказался null!");
            }
        }

        public bool AreEnemiesCleared()
        {
            Enemy[] remainingEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude);
            return remainingEnemies.Length == 0;
        }
    }
}