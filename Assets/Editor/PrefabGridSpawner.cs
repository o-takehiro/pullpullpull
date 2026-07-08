using UnityEngine;
using UnityEditor;

public class PrefabGridSpawner : EditorWindow {
    private GameObject prefab;
    private GameObject parentObject;

    private int countX = 1;
    private int countY = 1;
    private int countZ = 1;

    private bool clearChildrenBeforeSpawn = false;

    [MenuItem("Tools/Prefab Grid Spawner")]
    public static void ShowWindow() {
        GetWindow<PrefabGridSpawner>("Prefab Grid Spawner");
    }

    private void OnGUI() {
        GUILayout.Label("Prefab Grid Spawner", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        prefab = (GameObject)EditorGUILayout.ObjectField(
            "Prefab",
            prefab,
            typeof(GameObject),
            false);

        parentObject = (GameObject)EditorGUILayout.ObjectField(
            "親オブジェクト",
            parentObject,
            typeof(GameObject),
            true);

        if (GUILayout.Button("選択中を親に設定")) {
            parentObject = Selection.activeGameObject;
        }

        EditorGUILayout.Space();

        countX = EditorGUILayout.IntField("X個数", countX);
        countY = EditorGUILayout.IntField("Y個数", countY);
        countZ = EditorGUILayout.IntField("Z個数", countZ);

        EditorGUILayout.Space();

        clearChildrenBeforeSpawn = EditorGUILayout.Toggle(
            "生成前に子を削除",
            clearChildrenBeforeSpawn);

        EditorGUILayout.Space();

        GUI.enabled = prefab != null && parentObject != null;

        if (GUILayout.Button("生成")) {
            SpawnGrid();
        }

        GUI.enabled = true;

        if (parentObject != null) {
            EditorGUILayout.HelpBox(
                $"親 : {parentObject.name}",
                MessageType.Info);
        }
    }

    private void SpawnGrid() {
        Renderer renderer = prefab.GetComponentInChildren<Renderer>();

        if (renderer == null) {
            Debug.LogError("Prefab内にRendererが見つかりません");
            return;
        }

        Vector3 size = renderer.bounds.size;

        Undo.RegisterCompleteObjectUndo(
            parentObject,
            "Spawn Grid");

        if (clearChildrenBeforeSpawn) {
            for (int i = parentObject.transform.childCount - 1; i >= 0; i--) {
                Undo.DestroyObjectImmediate(
                    parentObject.transform.GetChild(i).gameObject);
            }
        }

        int createdCount = 0;

        for (int x = 0; x < countX; x++) {
            for (int y = 0; y < countY; y++) {
                for (int z = 0; z < countZ; z++) {
                    GameObject obj =
                        (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                    Undo.RegisterCreatedObjectUndo(
                        obj,
                        "Create Prefab");

                    obj.transform.SetParent(
                        parentObject.transform,
                        false);

                    obj.transform.localPosition = new Vector3(
                        x * size.x,
                        y * size.y,
                        z * size.z);

                    createdCount++;
                }
            }
        }

        Debug.Log($"生成完了 : {createdCount}個");
    }
}