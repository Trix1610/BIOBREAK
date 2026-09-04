using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawn;

    private void Start()
    {
        GameObject existingPlayer =
            GameObject.FindGameObjectWithTag("Player");

        Transform spawnPoint = GetSpawnPoint();

        if (existingPlayer == null)
        {
            Instantiate(
                playerPrefab,
                spawnPoint.position,
                Quaternion.identity
            );
        }
        else
        {
            existingPlayer.transform.position =
                spawnPoint.position;
        }

        CharacterSpawnData.Clear();
    }

    private Transform GetSpawnPoint()
    {
        if (!string.IsNullOrEmpty(CharacterSpawnData.SpawnName))
        {
            GameObject spawnObject =
                GameObject.Find(CharacterSpawnData.SpawnName);

            if (spawnObject != null)
                return spawnObject.transform;
        }

        return playerSpawn;
    }
}
