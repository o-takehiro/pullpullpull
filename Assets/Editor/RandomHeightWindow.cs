using UnityEngine;
using UnityEditor;

public class RandomHeightWindow : EditorWindow {
    private float minY = 5.0f;
    private float maxY = 8.0f;

    [MenuItem("Tools/Random Height Window")]
    public static void ShowWindow() {
        GetWindow<RandomHeightWindow>("Random Height");
    }

    private void OnGUI() {
        GUILayout.Label("選択オブジェクトの高さスケールをランダム化", EditorStyles.boldLabel);

        minY = EditorGUILayout.FloatField("Min Scale Y", minY);
        maxY = EditorGUILayout.FloatField("Max Scale Y", maxY);

        if (GUILayout.Button("ランダム化")) {
            foreach (GameObject obj in Selection.gameObjects) {
                Undo.RecordObject(obj.transform, "Random Height Scale");

                Vector3 scale = obj.transform.localScale;
                scale.y = Random.Range(minY, maxY);
                obj.transform.localScale = scale;
            }
        }
    }
}