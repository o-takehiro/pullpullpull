using UnityEngine;
using UnityEditor;

public class PrefabGridSpawner : EditorWindow {
    private GameObject prefab;

    private int countX = 1;
    private int countY = 1;
    private int countZ = 1;

    [MenuItem("Tools/Prefab Grid Spawner")]
    public static void ShowWindow() {
        GetWindow<PrefabGridSpawner>("Prefab Grid Spawner");
    }

    private void OnGUI() {
        GUILayout.Label("Prefab自動配置ツール", EditorStyles.boldLabel);

        prefab = (GameObject)EditorGUILayout.ObjectField(
            "Prefab",
            prefab,
            typeof(GameObject),
            false);

        EditorGUILayout.Space();

        countX = EditorGUILayout.IntField("X個数", countX);
        countY = EditorGUILayout.IntField("Y個数", countY);
        countZ = EditorGUILayout.IntField("Z個数", countZ);

        EditorGUILayout.Space();

        GUI.enabled = prefab != null;

        if (GUILayout.Button("生成")) {
            SpawnGrid();
        }

        GUI.enabled = true;
    }

    private void SpawnGrid() {
        Renderer renderer = prefab.GetComponentInChildren<Renderer>();

        if (renderer == null) {
            Debug.LogError("Prefab内にRendererが見つかりません。");
            return;
        }

        Vector3 size = renderer.bounds.size;

        GameObject root = new GameObject(prefab.name + "_Grid");

        Undo.RegisterCreatedObjectUndo(root, "Create Grid");

        for (int x = 0; x < countX; x++) {
            for (int y = 0; y < countY; y++) {
                for (int z = 0; z < countZ; z++) {
                    GameObject obj =
                        (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                    obj.transform.position = new Vector3(
                        x * size.x,
                        y * size.y,
                        z * size.z
                    );

                    obj.transform.SetParent(root.transform);
                }
            }
        }

        Debug.Log($"生成完了 : {countX * countY * countZ} 個");
    }
}