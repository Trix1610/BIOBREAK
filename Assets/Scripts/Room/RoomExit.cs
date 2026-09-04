using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomExit : MonoBehaviour
{
    [Header("Exit")]
    [SerializeField] private string direction;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (RunManager.Instance == null)
        {
            Debug.LogError("RoomExit: RunManager.Instance is NULL.");
            return;
        }

        string currentRoom = SceneManager.GetActiveScene().name;

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

        CharacterSpawnData.SetSpawn(destinationSpawn);

        if (ScreenTransition.Instance == null)
        {
            Debug.LogError(
                "RoomExit: ScreenTransition.Instance is NULL."
            );

            return;
        }

        ScreenTransition.Instance.LoadSceneWithTransition(
            destinationRoom
        );
    }
}