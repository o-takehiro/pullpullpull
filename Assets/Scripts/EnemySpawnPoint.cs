using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType{
    Walk,
    Bomb,
    Shot,
    Tail
}

public class EnemySpawnPoint : MonoBehaviour{
    [SerializeField]
    public EnemyType enemyType;
    [SerializeField]
    public float wanderingRadius = 1000;
    [SerializeField]
    public float rotationY = 0;
}
