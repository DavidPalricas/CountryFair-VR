using UnityEditor;

public static class ReserializeAll
{
    [MenuItem("Tools/Force Reserialize All Assets")]
    public static void Run()
    {
        AssetDatabase.ForceReserializeAssets();
    }
}