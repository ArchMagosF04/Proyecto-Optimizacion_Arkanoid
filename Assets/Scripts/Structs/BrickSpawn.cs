using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BrickSpawn
{
    public Vector2 SpawnPoint;
    public BrickType Type;
}

public enum BrickType 
{
    Glass, Wood, Stone, Metal
}
