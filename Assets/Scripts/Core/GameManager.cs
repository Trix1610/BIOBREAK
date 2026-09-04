using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Spawner Settings")]
    [SerializeField] private GameObject enemyPrefab; // Префаб врага
    [SerializeField] private LayerMask groundLayer;   // Слой земли/платформ
    [SerializeField] private float spawnYOffset = 1f; // Высота спавна над землей

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
        if (scene.name == "ROOM_00" || scene.name == "GAME")
        {
            return;
        }

        StartCoroutine(SpawnEnemiesDelayed());
    }

    private IEnumerator SpawnEnemiesDelayed()
    {
        // Ждем 0.2 секунды, пока комната и все ее объекты на 100% прогрузятся
        yield return new WaitForSeconds(0.2f);
        SpawnEnemiesOnGround();
    }

    private void SpawnEnemiesOnGround()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[GameManager] Не задан префаб врага в инспекторе GameManager!");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        int enemiesCount = Random.Range(3, 6);

        // Исправлено: убран устаревший параметр FindObjectsSortMode.None
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

        Debug.Log($"[GameManager] Сканирование комнаты {currentScene} по слою Ground: найдено объектов: {validPlatforms.Count}");

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

                Debug.Log($"-> [СПАВН ПО СЛОЮ] Враг #{i + 1} создан на объекте '{randomPlatform.name}' | Координаты: {spawnPosition}");
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] Не найдено объектов на слое Ground в {currentScene}!");
        }
    }

    public bool AreEnemiesCleared()
    {
        // Исправлено: убран устаревший параметр FindObjectsSortMode.None
        Enemy[] remainingEnemies = FindObjectsByType<Enemy>(FindObjectsInactive.Exclude);
        return remainingEnemies.Length == 0;
    }
}