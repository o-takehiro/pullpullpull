using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrefabGridSpawner : EditorWindow {
    public enum SpawnMode {
        Alternate, // 交互配置
        Random     // ランダム配置
    }

    private List<GameObject> prefabs = new();

    private GameObject parentObject;

    private int countX = 1;
    private int countY = 1;
    private int countZ = 1;

    private bool clearChildrenBeforeSpawn = false;

    private SpawnMode spawnMode = SpawnMode.Alternate;

    [MenuItem("Tools/Prefab Grid Spawner")]
    public static void ShowWindow() {
        GetWindow<PrefabGridSpawner>("Prefab Grid Spawner");
    }

    private void OnGUI() {
        GUILayout.Label(
            "Prefab Grid Spawner",
            EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // Prefab数
        int prefabCount = Mathf.Max(
            1,
            EditorGUILayout.IntField(
                "Prefab数",
                prefabs.Count == 0 ? 1 : prefabs.Count));

        while (prefabs.Count < prefabCount)
            prefabs.Add(null);

        while (prefabs.Count > prefabCount)
            prefabs.RemoveAt(prefabs.Count - 1);

        EditorGUILayout.LabelField(
            "Prefab List",
            EditorStyles.boldLabel);

        for (int i = 0; i < prefabs.Count; i++) {
            prefabs[i] = (GameObject)EditorGUILayout.ObjectField(
                $"Prefab {i}",
                prefabs[i],
                typeof(GameObject),
                false);
        }

        EditorGUILayout.Space();

        spawnMode = (SpawnMode)EditorGUILayout.EnumPopup(
            "配置方法",
            spawnMode);

        EditorGUILayout.Space();

        parentObject = (GameObject)EditorGUILayout.ObjectField(
            "親オブジェクト",
            parentObject,
            typeof(GameObject),
            true);

        if (GUILayout.Button("選択中を親に設定")) {
            parentObject = Selection.activeGameObject;
        }

        EditorGUILayout.Space();

        countX = Mathf.Max(
            1,
            EditorGUILayout.IntField(
                "X個数",
                countX));

        countY = Mathf.Max(
            1,
            EditorGUILayout.IntField(
                "Y個数",
                countY));

        countZ = Mathf.Max(
            1,
            EditorGUILayout.IntField(
                "Z個数",
                countZ));

        EditorGUILayout.Space();

        clearChildrenBeforeSpawn = EditorGUILayout.Toggle(
            "生成前に子を削除",
            clearChildrenBeforeSpawn);

        EditorGUILayout.Space();

        bool hasPrefab = false;

        foreach (var prefab in prefabs) {
            if (prefab != null) {
                hasPrefab = true;
                break;
            }
        }

        GUI.enabled =
            hasPrefab &&
            parentObject != null;

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
        List<GameObject> validPrefabs = new();

        foreach (var prefab in prefabs) {
            if (prefab != null) {
                validPrefabs.Add(prefab);
            }
        }

        if (validPrefabs.Count == 0) {
            Debug.LogError("Prefabが設定されていません");
            return;
        }

        Renderer renderer =
            validPrefabs[0].GetComponentInChildren<Renderer>();

        if (renderer == null) {
            Debug.LogError(
                "Prefab内にRendererが見つかりません");
            return;
        }

        Vector3 size = renderer.bounds.size;

        Undo.RegisterCompleteObjectUndo(
            parentObject,
            "Spawn Grid");

        if (clearChildrenBeforeSpawn) {
            for (int i = parentObject.transform.childCount - 1;
                 i >= 0;
                 i--) {
                Undo.DestroyObjectImmediate(
                    parentObject.transform
                        .GetChild(i)
                        .gameObject);
            }
        }

        int createdCount = 0;
        int alternateIndex = 0;

        for (int x = 0; x < countX; x++) {
            for (int y = 0; y < countY; y++) {
                for (int z = 0; z < countZ; z++) {
                    GameObject selectedPrefab = null;

                    switch (spawnMode) {
                        case SpawnMode.Alternate:
                            selectedPrefab =
                                validPrefabs[
                                    alternateIndex %
                                    validPrefabs.Count];

                            alternateIndex++;
                            break;

                        case SpawnMode.Random:
                            selectedPrefab =
                                validPrefabs[
                                    Random.Range(
                                        0,
                                        validPrefabs.Count)];
                            break;
                    }

                    GameObject obj =
                        (GameObject)PrefabUtility
                        .InstantiatePrefab(
                            selectedPrefab);

                    Undo.RegisterCreatedObjectUndo(
                        obj,
                        "Create Prefab");

                    obj.transform.SetParent(
                        parentObject.transform,
                        false);

                    obj.transform.localPosition =
                        new Vector3(
                            x * size.x,
                            y * size.y,
                            z * size.z);

                    createdCount++;
                }
            }
        }

        Debug.Log(
            $"生成完了 : {createdCount}個 ({spawnMode})");
    }
}