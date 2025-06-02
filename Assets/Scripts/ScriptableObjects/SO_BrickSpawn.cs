using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpawnData", menuName = "Data/Game/SpawnData", order = 0)]
public class SO_BrickSpawn : ScriptableObject
{
    [field: SerializeField] public BrickSpawn[] spawnList;
}
