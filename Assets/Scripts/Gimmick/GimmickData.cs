using System;
using System.Collections.Generic;

[Serializable]
public class LeverData {
    public int LeverID;
    public string Position;
    public float Rotation;
}

[Serializable]
public class GimmickData {
    public string Type;
    public int LeverID;
    public string Position;
    public float Rotation;
}

[Serializable]
public class StageGimmickData {
    public List<LeverData> Levers = new();
    public List<GimmickData> Gimmicks = new();
}

/// <summary>
/// ギミックタイプ
/// </summary>
public enum GimmickType {
    BreakWall,
    BomBreakWall,
    HookShot,
    ExitArea,
    PullOutFloor,
    Turret
}