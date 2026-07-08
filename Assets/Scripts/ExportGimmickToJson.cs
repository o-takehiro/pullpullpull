using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteInEditMode]
public class ExportGimmickToJson : MonoBehaviour {
    [Header("JSON出力先")]
    [SerializeField]
    private string outputDirectory;

    [ContextMenu("Export Gimmick Json")]
    void Export() {
        StageGimmickData stageData = new();

        //--------------------------------------
        // レバー
        //--------------------------------------

        GameObject[] objs = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in objs) {
            if (obj.name.StartsWith("LeverPoint") ||
                obj.name.StartsWith("Lever_SP")) {
                LeverPoint lever = obj.GetComponent<LeverPoint>();

                if (lever == null)
                    continue;

                stageData.Levers.Add(new LeverData() {
                    LeverID = lever.leverID,
                    Position = obj.name,
                    Rotation = obj.transform.eulerAngles.y
                });
            }
        }

        //--------------------------------------
        // ギミック
        //--------------------------------------

        GimmickPoint[] gimmicks = FindObjectsOfType<GimmickPoint>();

        foreach (var gimmick in gimmicks) {
            stageData.Gimmicks.Add(new GimmickData() {
                Type = gimmick.gimmickType.ToString(),

                LeverID = gimmick.leverID,

                Position = gimmick.name,

                Rotation = gimmick.transform.eulerAngles.y
            });
        }

        string json = JsonUtility.ToJson(stageData, true);

        string dir = Path.Combine(
            outputDirectory,
            "PullProject",
            "src",
            "Data"
        );

        Directory.CreateDirectory(dir);

        string stageName = transform.parent != null
            ? transform.parent.name + "_GimmickData"
            : "Stage";

        string path = Path.Combine(dir, stageName + ".json");

        File.WriteAllText(path, json);

        Debug.Log(json);
        Debug.Log(path);
    }
}
