using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


[ExecuteInEditMode]
public class ExportCoinToJson : MonoBehaviour {


    [Header("JSON出力先")]
    [SerializeField]
    private string outputDirectory;


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

    [System.Serializable]
    struct StageCoinData {
        public string frameName;

        public StageCoinData(string frameName) {
            this.frameName = frameName;
        }
    }

    [System.Serializable]
    class CoinDataList {
        public List<StageCoinData> data = new();
    }

    private string prefabName = "Coin_SP{0}";
    private int searchMaxCount = 100;

    [ContextMenu("Export StageCollisionData (Optimized)")]
    void Export() {
        GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();

        CoinDataList coinSpawnPoints = new();
        // シーンにあるスポーンポイントを名前から取得
        for (int i = 0; i < searchMaxCount; i++) {
            string name = string.Format(prefabName, i);
            GameObject obj = GameObject.Find(name);
            if (obj != null) {

                StageCoinData data = new StageCoinData(name);

                coinSpawnPoints.data.Add(data);
            } else
                // Nullだったら最後のスポーンポイントとして抜ける
                break;
        }

        string json = JsonUtility.ToJson(coinSpawnPoints, true);

        if (string.IsNullOrEmpty(outputDirectory)) {
            Debug.LogError("JSON出力先が設定されていません");
            return;
        }

        string dir = Path.Combine(
            outputDirectory
        );

        Directory.CreateDirectory(dir);

        string stageName = transform.parent != null
            ? transform.parent.name + "_CoinData"
            : "Stage";

        string path = Path.Combine(
            dir,
            stageName + ".json"
        );

        File.WriteAllText(path, json);

        Debug.Log("✅ 出力数: " + coinSpawnPoints.data.Count);
        Debug.Log("📂 出力先: " + path);

    }

}