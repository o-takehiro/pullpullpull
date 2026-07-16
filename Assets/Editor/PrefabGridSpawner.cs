using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrefabGridSpawner : EditorWindow {

    public enum SpawnMode {
        Alternate,
        Random,
        Road,
        Circle,
        FilledCircle
    }

    private List<GameObject> prefabs = new();

    private GameObject parentObject;

    private int countX = 1;
    private int countY = 1;
    private int countZ = 1;

    private float circleRadius = 10f;
    private int circleCount = 16;

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

        if (spawnMode == SpawnMode.Circle) {

            circleRadius = Mathf.Max(
                1f,
                EditorGUILayout.FloatField(
                    "半径",
                    circleRadius));

            circleCount = Mathf.Max(
                3,
                EditorGUILayout.IntField(
                    "配置数",
                    circleCount));
        }

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


    private void SpawnCircle(
        List<GameObject> validPrefabs) {
        int alternateIndex = 0;

        for (int i = 0; i < circleCount; i++) {
            float angle =
                (360f / circleCount) * i;

            float rad =
                angle * Mathf.Deg2Rad;

            Vector3 position =
                new Vector3(
                    Mathf.Cos(rad) * circleRadius,
                    0f,
                    Mathf.Sin(rad) * circleRadius);

            GameObject selectedPrefab =
                validPrefabs[
                    alternateIndex %
                    validPrefabs.Count];

            alternateIndex++;

            GameObject obj =
                (GameObject)PrefabUtility
                .InstantiatePrefab(
                    selectedPrefab);

            Undo.RegisterCreatedObjectUndo(
                obj,
                "Create Circle");

            obj.transform.SetParent(
                parentObject.transform,
                false);

            obj.transform.localPosition =
                position;
        }
    }

    private void SpawnFilledCircle(
    List<GameObject> validPrefabs,
    Vector3 size) {
        int alternateIndex = 0;

        for (float x = -circleRadius;
             x <= circleRadius;
             x += size.x) {
            for (float z = -circleRadius;
                 z <= circleRadius;
                 z += size.z) {
                if ((x * x) + (z * z) >
                    circleRadius * circleRadius) {
                    continue;
                }

                GameObject selectedPrefab =
                    validPrefabs[
                        alternateIndex %
                        validPrefabs.Count];

                alternateIndex++;

                GameObject obj =
                    (GameObject)PrefabUtility
                    .InstantiatePrefab(
                        selectedPrefab);

                Undo.RegisterCreatedObjectUndo(
                    obj,
                    "Create Filled Circle");

                obj.transform.SetParent(
                    parentObject.transform,
                    false);

                obj.transform.localPosition =
                    new Vector3(
                        x,
                        0,
                        z);
            }
        }
    }

    private void SpawnRandomWalk(
    List<GameObject> validPrefabs,
    Vector3 size) {
        int length = countX;

        Vector3Int current = Vector3Int.zero;

        HashSet<Vector3Int> used = new();

        for (int i = 0; i < length; i++) {
            GameObject selectedPrefab =
                validPrefabs[
                    Random.Range(
                        0,
                        validPrefabs.Count)];

            GameObject obj =
                (GameObject)PrefabUtility
                .InstantiatePrefab(selectedPrefab);

            Undo.RegisterCreatedObjectUndo(
                obj,
                "Create Path");

            obj.transform.SetParent(
                parentObject.transform,
                false);

            obj.transform.localPosition =
                new Vector3(
                    current.x * size.x,
                    current.y * size.y,
                    current.z * size.z);

            used.Add(current);

            List<Vector3Int> directions =
                new List<Vector3Int>()
                {
                Vector3Int.right,
                Vector3Int.left,
                Vector3Int.forward,
                Vector3Int.back
                };

            for (int j = directions.Count - 1;
                 j > 0;
                 j--) {
                int k = Random.Range(0, j + 1);

                (directions[j], directions[k]) =
                    (directions[k], directions[j]);
            }

            foreach (var dir in directions) {
                Vector3Int next = current + dir;

                if (!used.Contains(next)) {
                    current = next;
                    break;
                }
            }
        }

        Debug.Log($"道生成完了 : {length}個");
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


        if (spawnMode == SpawnMode.Road) {
            SpawnRandomWalk(
                validPrefabs,
                size);

            return;
        }


        Undo.RegisterCompleteObjectUndo(
        parentObject,
        "Spawn Grid");


        if (spawnMode == SpawnMode.Circle) {
            SpawnCircle(
                validPrefabs);

            return;
        }

        if (spawnMode == SpawnMode.FilledCircle) {
            SpawnFilledCircle(
                validPrefabs,
                size);

            return;
        }

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