using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

enum Direction {
    PosX, NegX,
    PosY, NegY,
    PosZ, NegZ
}


[ExecuteInEditMode]
public class ExportLevelToJson : MonoBehaviour {
    [System.Serializable]
    public class Block {
        public float minX, minY, minZ;
        public float maxX, maxY, maxZ;
        public string type;
    }

    [System.Serializable]
    public class BlockList {
        public List<Block> blocks = new List<Block>();
    }

    [ContextMenu("Export StageCollisionData (Optimized)")]
    void Export() {
        GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();

        BlockList data = new BlockList();
        List<Block> groundBlocks = new List<Block>();

        foreach (GameObject obj in objs) {
            if (!(obj.CompareTag("Ground") ||
                  obj.CompareTag("Bridge") ||
                  obj.CompareTag("Fence") ||
                  obj.CompareTag("Tree") ||
                  obj.CompareTag("SubGround") ||
                  obj.CompareTag("Wall") ||
                  obj.CompareTag("box")))
                continue;

            BoxCollider box = obj.GetComponent<BoxCollider>();
            if (box == null) {
                Debug.LogWarning("BoxCollider無し: " + obj.name);
                continue;
            }

            Block b = CreateBlock(obj, box);

            // Groundだけ後でまとめる
            if (b.type == "ground")
                groundBlocks.Add(b);
            else
                data.blocks.Add(b);
        }

        // ✅ Groundをマージ
        List<Block> mergedGround = MergeGroundBlocks(groundBlocks);
        mergedGround = RemoveFullySurrounded(mergedGround);
        data.blocks.AddRange(mergedGround);

        string json = JsonUtility.ToJson(data, true);

        string desktop = System.Environment.GetFolderPath(
            System.Environment.SpecialFolder.Desktop);

        string dir = desktop + "/就活用/Pull/PullProject/src/Data/";
        Directory.CreateDirectory(dir);

        string stageName = transform.parent != null
            ? transform.parent.name
            : "Stage";

        string path = dir + stageName + ".json";

        File.WriteAllText(path, json);

        Debug.Log("✅ 出力数: " + data.blocks.Count);
        Debug.Log("📂 出力先: " + path);
    }

    // ===========================
    // Block作成（数値丸め込み）
    // ===========================
    Block CreateBlock(GameObject obj, BoxCollider box) {
        // ✅ 回転込みAABB取得（これが核心）
        Bounds bounds = box.bounds;

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        return new Block
        {
            minX = Round(min.x),
            minY = Round(min.y),
            minZ = Round(min.z),
            maxX = Round(max.x),
            maxY = Round(max.y),
            maxZ = Round(max.z),
            type = obj.tag.ToLower()
        };
    }

    // ===========================
    // 数値丸め（超重要）
    // ===========================
    float Round(float v) {
        return Mathf.Round(v * 100f) / 100f; // 小数第2位
    }

    // ===========================
    // Ground簡易マージ
    // ===========================
    List<Block> MergeGroundBlocks(List<Block> blocks) {
        List<Block> result = new List<Block>();

        // Yでグループ（高さが同じものだけマージ）
        var groups = blocks.GroupBy(b => b.minY).ToList();

        foreach (var group in groups) {
            var list = group.ToList();

            while (list.Count > 0) {
                Block current = list[0];
                list.RemoveAt(0);

                for (int i = list.Count - 1; i >= 0; i--) {
                    if (CanMerge(current, list[i])) {
                        current = Merge(current, list[i]);
                        list.RemoveAt(i);
                    }
                }

                result.Add(current);
            }
        }

        return result;
    }

    bool CanMerge(Block a, Block b) {
        // 高さ一致
        if (!Mathf.Approximately(a.minY, b.minY) ||
            !Mathf.Approximately(a.maxY, b.maxY))
            return false;

        // Z範囲一致 → X方向に並んでる
        bool sameZ = Mathf.Approximately(a.minZ, b.minZ) &&
                     Mathf.Approximately(a.maxZ, b.maxZ);

        bool adjacentX = Mathf.Approximately(a.maxX, b.minX) ||
                         Mathf.Approximately(b.maxX, a.minX);

        // X範囲一致 → Z方向に並んでる
        bool sameX = Mathf.Approximately(a.minX, b.minX) &&
                     Mathf.Approximately(a.maxX, b.maxX);

        bool adjacentZ = Mathf.Approximately(a.maxZ, b.minZ) ||
                         Mathf.Approximately(b.maxZ, a.minZ);

        return (sameZ && adjacentX) || (sameX && adjacentZ);
    }

    Block Merge(Block a, Block b) {
        return new Block
        {
            minX = Mathf.Min(a.minX, b.minX),
            minY = Mathf.Min(a.minY, b.minY),
            minZ = Mathf.Min(a.minZ, b.minZ),

            maxX = Mathf.Max(a.maxX, b.maxX),
            maxY = Mathf.Max(a.maxY, b.maxY),
            maxZ = Mathf.Max(a.maxZ, b.maxZ),

            type = "ground"
        };
    }


    List<Block> RemoveFullySurrounded(List<Block> blocks) {
        List<Block> result = new List<Block>();

        foreach (var a in blocks) {
            if (!IsFullySurrounded(a, blocks))
                result.Add(a);
        }

        Debug.Log("削除後Ground数: " + result.Count);
        return result;
    }

    bool IsFullySurrounded(Block a, List<Block> blocks) {
        return HasNeighbor(a, blocks, Direction.PosX) &&
               HasNeighbor(a, blocks, Direction.NegX) &&
               HasNeighbor(a, blocks, Direction.PosY) &&
               HasNeighbor(a, blocks, Direction.NegY) &&
               HasNeighbor(a, blocks, Direction.PosZ) &&
               HasNeighbor(a, blocks, Direction.NegZ);
    }


    bool HasNeighbor(Block a, List<Block> blocks, Direction dir) {
        foreach (var b in blocks) {
            if (a == b) continue;

            if (IsTouching(a, b, dir))
                return true;
        }
        return false;
    }

    bool IsTouching(Block a, Block b, Direction dir) {
        float eps = 0.01f;

        switch (dir) {
            case Direction.PosX:
                return Mathf.Abs(a.maxX - b.minX) < eps &&
                       Overlap(a.minY, a.maxY, b.minY, b.maxY) &&
                       Overlap(a.minZ, a.maxZ, b.minZ, b.maxZ);

            case Direction.NegX:
                return Mathf.Abs(a.minX - b.maxX) < eps &&
                       Overlap(a.minY, a.maxY, b.minY, b.maxY) &&
                       Overlap(a.minZ, a.maxZ, b.minZ, b.maxZ);

            case Direction.PosY:
                return Mathf.Abs(a.maxY - b.minY) < eps &&
                       Overlap(a.minX, a.maxX, b.minX, b.maxX) &&
                       Overlap(a.minZ, a.maxZ, b.minZ, b.maxZ);

            case Direction.NegY:
                return Mathf.Abs(a.minY - b.maxY) < eps &&
                       Overlap(a.minX, a.maxX, b.minX, b.maxX) &&
                       Overlap(a.minZ, a.maxZ, b.minZ, b.maxZ);

            case Direction.PosZ:
                return Mathf.Abs(a.maxZ - b.minZ) < eps &&
                       Overlap(a.minX, a.maxX, b.minX, b.maxX) &&
                       Overlap(a.minY, a.maxY, b.minY, b.maxY);

            case Direction.NegZ:
                return Mathf.Abs(a.minZ - b.maxZ) < eps &&
                       Overlap(a.minX, a.maxX, b.minX, b.maxX) &&
                       Overlap(a.minY, a.maxY, b.minY, b.maxY);
        }

        return false;
    }


    bool Overlap(float aMin, float aMax, float bMin, float bMax) {
        return aMax > bMin && aMin < bMax;
    }


}