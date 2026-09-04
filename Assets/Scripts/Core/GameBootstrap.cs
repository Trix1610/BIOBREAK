using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("ROOM_00");
    }
}