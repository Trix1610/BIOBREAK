using UnityEngine;
using Core;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene(SceneNames.StartRoom);
    }
}