using UnityEngine;
using UnityEditor;

public class CleanupMissingScripts : EditorWindow
{
    [MenuItem("Tools/Cleanup Missing Scripts from Player Prefab")]
    public static void CleanupPlayerPrefab()
    {
        string playerPrefabPath = "Assets/Prefabs/Characters/Player.prefab";
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);

        if (playerPrefab == null)
        {
            Debug.LogWarning("Player prefab not found at: " + playerPrefabPath);
            return;
        }

        int removedCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(playerPrefab);

        if (removedCount > 0)
        {
            EditorUtility.SetDirty(playerPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log($"Removed {removedCount} missing scripts from Player prefab");
        }
        else
        {
            Debug.Log("No missing scripts found on Player prefab");
        }
    }
}
