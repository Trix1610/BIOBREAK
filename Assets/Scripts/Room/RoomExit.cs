using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomExit : MonoBehaviour
{
    [Header("Exit")]
    [SerializeField] private string direction;

    private bool isUsed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (isUsed)
            return;

        // Проверяем, уничтожены ли все враги в комнате
        if (GameManager.Instance != null && !GameManager.Instance.AreEnemiesCleared())
        {
            Debug.Log("Дверь заблокирована! Сначала уничтожьте всех врагов.");
            return;
        }

        if (RunManager.Instance == null)
        {
            Debug.LogError(
                "RoomExit: RunManager.Instance is NULL."
            );

            return;
        }

        string currentRoom =
            SceneManager.GetActiveScene().name;

        string destinationRoom =
            RunManager.Instance.GetDestination(
                currentRoom,
                direction
            );

        if (string.IsNullOrEmpty(destinationRoom))
        {
            Debug.LogError(
                $"RoomExit: destination not found for " +
                $"{currentRoom} -> {direction}"
            );

            return;
        }

        string destinationSpawn;

        if (direction == "Right")
        {
            destinationSpawn = "Spawn_Left";
        }
        else if (direction == "Left")
        {
            destinationSpawn = "Spawn_Right";
        }
        else
        {
            Debug.LogError(
                $"RoomExit: unknown direction '{direction}'."
            );

            return;
        }

        if (ScreenTransition.Instance == null)
        {
            Debug.LogError(
                "RoomExit: ScreenTransition.Instance is NULL."
            );

            return;
        }

        isUsed = true;

        CharacterSpawnData.SetSpawn(destinationSpawn);

        ScreenTransition.Instance.LoadSceneWithTransition(
            destinationRoom
        );
    }
}