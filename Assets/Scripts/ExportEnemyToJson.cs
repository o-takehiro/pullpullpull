using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[System.Serializable]
struct StageEnemyData {
    public string frameName;
    public EnemyType enemyType;

    public StageEnemyData(string frameName, EnemyType enemyType) {
        this.frameName = frameName;
        this.enemyType = enemyType;
    }
}

[System.Serializable]
class EnemyDataList {
    public List<StageEnemyData> data = new();
}


[ExecuteInEditMode]
public class ExportEnemyToJson : MonoBehaviour {

    [Header("JSON出力先")]
    [SerializeField]
    private string outputDirectory;

    private string prefabName = "Enemy_SP{}";
    private int searchMaxCount = 100;

#if UNITY_EDITOR
    [ContextMenu("Select Output Folder")]
    void SelectOutputFolder() {
        string selected = EditorUtility.OpenFolderPanel(
            "PullProjectの親フォルダを選択",
            "",
            ""
        );

        if (!string.IsNullOrEmpty(selected)) {
            outputDirectory = selected;
            Debug.Log("選択: " + outputDirectory);
        }
    }
#endif


    [ContextMenu("Export StageCollisionData (Optimized)")]
    void Export() {
        EnemyDataList enemySpawnPoints = new();
        // シーンにあるスポーンポイントを名前から取得
        for (int i = 0; i < searchMaxCount; i++) {
            string name = string.Format("Enemy_SP{0}", i);
            GameObject obj = GameObject.Find(name);
            if (obj != null) {
                // EnemySpawnPointの取得
                EnemySpawnPoint enemySpawnPoint = obj.GetComponent<EnemySpawnPoint>();
                StageEnemyData data;
                // 取得出来たらEnemyTypeをもらう
                if (enemySpawnPoint != null)
                    data = new StageEnemyData(name, enemySpawnPoint.enemyType);
                else
                    //取得できなかったらWalkとする
                    data = new StageEnemyData(name, EnemyType.Walk);
                ;

                enemySpawnPoints.data.Add(data);
            } else
                // Nullだったら最後のスポーンポイントとして抜ける
                break;
        }

        string json = JsonUtility.ToJson(enemySpawnPoints, true);

        if (string.IsNullOrEmpty(outputDirectory)) {
            Debug.LogError("JSON出力先が設定されていません");
            return;
        }

        string dir = Path.Combine(
            outputDirectory,
            "PullProject",
            "src",
            "Data"
        );

        Directory.CreateDirectory(dir);

        string stageName = transform.parent != null
            ? transform.parent.name + "_EnemyData"
            : "Stage";

        string path = Path.Combine(
            dir,
            stageName + ".json"
        );

        File.WriteAllText(path, json);

        Debug.Log("✅ 出力数: " + enemySpawnPoints.data.Count);
        Debug.Log("📂 出力先: " + path);
    }
}
