using UnityEngine;
using UnityEditor;
using Core;

[InitializeOnLoad]
public class SetupPlayerReference
{
    static SetupPlayerReference()
    {
        EditorApplication.delayCall += AddPlayerReferenceToScene;
    }

    private static void AddPlayerReferenceToScene()
    {
        // Проверяем, есть ли уже PlayerReference в сцене
        PlayerReference existing = Object.FindAnyObjectByType<PlayerReference>();
        if (existing != null)
        {
            Debug.Log("PlayerReference already exists in scene");
            return;
        }

        // Создаем новый GameObject с PlayerReference
        GameObject playerRefObj = new GameObject("PlayerReference");
        playerRefObj.AddComponent<PlayerReference>();
        Debug.Log("PlayerReference added to scene");
    }
}
