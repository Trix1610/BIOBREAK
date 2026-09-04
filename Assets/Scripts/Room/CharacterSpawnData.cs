public static class CharacterSpawnData
{
    private static string spawnName;

    public static string SpawnName => spawnName;

    public static void SetSpawn(string name)
    {
        spawnName = name;
    }

    public static void Clear()
    {
        spawnName = null;
    }
}