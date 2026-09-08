using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string firstGameScene = "ROOM_00"; 

    public void OnStartGameClicked()
    {
        // Добавляем отладку, чтобы сразу увидеть в консоли нажатие
        Debug.Log("Кнопка Старт нажата!");

        if (RunManager.Instance != null)
        {
            RunManager.Instance.StartNewRun();
        }

        SceneManager.LoadScene(firstGameScene);
    }

    public void OnExitGameClicked()
    {
        Debug.Log("Кнопка Выход нажата!");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}