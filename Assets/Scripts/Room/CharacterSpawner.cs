using UnityEngine;
using Core;

public class CharacterSpawner : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform spawnDefault;
    [SerializeField] private Transform spawnLeft;
    [SerializeField] private Transform spawnRight;

    private void Start()
    {
        GameObject existingPlayer =
            GameObject.FindGameObjectWithTag("Player");

        Transform spawnPoint = GetSpawnPoint();

        if (spawnPoint == null)
        {
            Debug.LogError(
                "CharacterSpawner: Spawn Point is not assigned."
            );

            return;
        }

        if (existingPlayer == null)
        {
            if (playerPrefab == null)
            {
                Debug.LogError(
                    "CharacterSpawner: Player Prefab is not assigned."
                );

                return;
            }

            GameObject newPlayer = Instantiate(
                playerPrefab,
                spawnPoint.position,
                Quaternion.identity
            );
            
            PlayerReference.Instance?.SetPlayer(newPlayer);
            Debug.Log("CharacterSpawner: Игрок заспавнен и добавлен в PlayerReference");
        }
        else
        {
            existingPlayer.transform.position =
                spawnPoint.position;
            
            PlayerReference.Instance?.SetPlayer(existingPlayer);
            Debug.Log("CharacterSpawner: Существующий игрок перемещен и добавлен в PlayerReference");
        }

        CharacterSpawnData.Clear();
    }

    private Transform GetSpawnPoint()
    {
        switch (CharacterSpawnData.SpawnName)
        {
            case "Spawn_Left":
                return spawnLeft;

            case "Spawn_Right":
                return spawnRight;

            default:
                return spawnDefault;
        }
    }
}