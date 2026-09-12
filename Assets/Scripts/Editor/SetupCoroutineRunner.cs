using UnityEngine;
using UnityEditor;
using Core;

[InitializeOnLoad]
public class SetupCoroutineRunner
{
    static SetupCoroutineRunner()
    {
        EditorApplication.delayCall += AddCoroutineRunnerToScene;
    }

    private static void AddCoroutineRunnerToScene()
    {
        CoroutineRunner existing = Object.FindAnyObjectByType<CoroutineRunner>();
        if (existing != null)
        {
            Debug.Log("CoroutineRunner already exists in scene");
            return;
        }

        GameObject runnerObj = new GameObject("CoroutineRunner");
        runnerObj.AddComponent<CoroutineRunner>();
        Debug.Log("CoroutineRunner added to scene");
    }
}
