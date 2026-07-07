using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GimmickPoint : MonoBehaviour
{
    [Header("ギミックの種類")]
    public GimmickType gimmickType;

    [Header("レバーで動く場合のみ設定")]
    public int leverID = 0;
}
