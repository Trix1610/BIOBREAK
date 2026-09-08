using System.Collections.Generic;
using UnityEngine;

namespace Room
{
    public sealed class RoomEnemySpawner
    {
        private readonly GameObject enemyPrefab;
        private readonly LayerMask groundLayer;
        private readonly float spawnYOffset;

        public RoomEnemySpawner(
            GameObject enemyPrefab,
            LayerMask groundLayer,
            float spawnYOffset)
        {
            this.enemyPrefab = enemyPrefab;
            this.groundLayer = groundLayer;
            this.spawnYOffset = spawnYOffset;
        }

        public int Spawn()
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning("[RoomEnemySpawner] Не задан префаб врага.");
                return 0;
            }

            int enemyCount = Random.Range(3, 6);
            List<Transform> validPlatforms = FindValidPlatforms();
            int enemiesToSpawn = Mathf.Min(enemyCount, validPlatforms.Count);

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                int platformIndex = Random.Range(0, validPlatforms.Count);
                Transform platform = validPlatforms[platformIndex];
                validPlatforms.RemoveAt(platformIndex);

                Object.Instantiate(enemyPrefab, GetSpawnPosition(platform), Quaternion.identity);
            }

            if (enemiesToSpawn == 0)
            {
                Debug.LogWarning("[RoomEnemySpawner] Не найдены платформы на groundLayer.");
            }

            return enemiesToSpawn;
        }

        private List<Transform> FindValidPlatforms()
        {
            GameObject[] allObjects =
                Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
            List<Transform> validPlatforms = new();

            foreach (GameObject sceneObject in allObjects)
            {
                bool isGround = ((1 << sceneObject.layer) & groundLayer) != 0;

                if (isGround &&
                    !sceneObject.CompareTag("Player") &&
                    !sceneObject.CompareTag("Enemy"))
                {
                    validPlatforms.Add(sceneObject.transform);
                }
            }

            return validPlatforms;
        }

        private Vector2 GetSpawnPosition(Transform platform)
        {
            Collider2D collider = platform.GetComponent<Collider2D>();
            if (collider == null)
            {
                return new Vector2(
                    platform.position.x,
                    platform.position.y + spawnYOffset);
            }

            return new Vector2(
                Random.Range(collider.bounds.min.x, collider.bounds.max.x),
                collider.bounds.max.y + spawnYOffset);
        }
    }
}