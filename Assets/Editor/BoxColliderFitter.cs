

using UnityEditor;
using UnityEngine;

/// <summary>
/// オブジェクトに対して正確にBoxColliderをフィットさせるためのコンポーネント
/// Sekino
/// </summary>
public class BoxColliderFitter
{
    [MenuItem("Tools/Fit Single BoxCollider To Selection %#x")] // Ctrl+Shift+B
    static void FitSingleBoxCollider() {
        var gos = Selection.gameObjects;
        if (gos.Length == 0) {
            Debug.LogWarning("オブジェクトが選択されていません");
            return;
        }

        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;
        bool found = false;

        foreach (var go in gos) {
            var renderers = go.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers) {
                min = Vector3.Min(min, r.bounds.min);
                max = Vector3.Max(max, r.bounds.max);
                found = true;
            }
        }

        if (!found) {
            Debug.LogWarning("選択オブジェクトにRendererが見つかりません");
            return;
        }

        Vector3 worldCenter = (min + max) * 0.5f;
        Vector3 worldSize = max - min;

        // 新しい空オブジェクトをワールド空間(回転なし・スケール1)に生成
        GameObject colliderObj = new GameObject("CombinedCollider");
        Undo.RegisterCreatedObjectUndo(colliderObj, "Create Combined Collider");
        colliderObj.transform.position = worldCenter;
        colliderObj.transform.rotation = Quaternion.identity;
        colliderObj.transform.localScale = Vector3.one;

        BoxCollider box = colliderObj.AddComponent<BoxCollider>();
        box.center = Vector3.zero; // オブジェクト自体を中心に置いたのでcenterは0
        box.size = worldSize;

        Debug.Log("Complete!");
    }
}
