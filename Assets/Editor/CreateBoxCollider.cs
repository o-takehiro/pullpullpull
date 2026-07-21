using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CreateBoxCollider {
    [MenuItem("Tools/Fit Single BoxCollider To Selection %#q")]
    static void FitSingleBoxCollider() {
        var gos = Selection.gameObjects;

        if (gos.Length == 0) {
            Debug.LogWarning("オブジェクトが選択されていません");
            return;
        }

        var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

        Transform root = prefabStage != null
            ? prefabStage.prefabContentsRoot.transform
            : null;

        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;
        bool found = false;

        foreach (var go in gos) {
            var renderers = go.GetComponentsInChildren<Renderer>();

            foreach (var r in renderers) {
                Bounds b = r.bounds;

                Vector3[] corners =
                {
                    new(b.min.x, b.min.y, b.min.z),
                    new(b.min.x, b.min.y, b.max.z),
                    new(b.min.x, b.max.y, b.min.z),
                    new(b.min.x, b.max.y, b.max.z),
                    new(b.max.x, b.min.y, b.min.z),
                    new(b.max.x, b.min.y, b.max.z),
                    new(b.max.x, b.max.y, b.min.z),
                    new(b.max.x, b.max.y, b.max.z),
                };

                foreach (var p in corners) {
                    Vector3 local = root != null
                        ? root.InverseTransformPoint(p)
                        : p;

                    min = Vector3.Min(min, local);
                    max = Vector3.Max(max, local);
                }

                found = true;
            }
        }

        if (!found) {
            Debug.LogWarning("Rendererが見つかりません");
            return;
        }

        Vector3 center = (min + max) * 0.5f;
        Vector3 size = max - min;

        GameObject colliderObj = new("CombinedCollider");

        Undo.RegisterCreatedObjectUndo(
            colliderObj,
            "Create Combined Collider"
        );

        if (root != null) {
            colliderObj.transform.SetParent(root, false);

            var box = colliderObj.AddComponent<BoxCollider>();
            box.center = center;
            box.size = size;
        }
        else {
            colliderObj.transform.position = center;

            var box = colliderObj.AddComponent<BoxCollider>();
            box.center = Vector3.zero;
            box.size = size;
        }

        Debug.Log("Complete!");
    }
}