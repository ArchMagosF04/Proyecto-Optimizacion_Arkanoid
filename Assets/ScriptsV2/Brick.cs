using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : GameEntity
{
    public float topSide {  get; private set; }
    public float bottomSide { get; private set; }
    public float rightSide { get; private set; }
    public float leftSide { get; private set; }

    public Brick(Transform transform)
    {
       this.transform = transform;

        topSide = transform.position.y + transform.lossyScale.y / 2;
        bottomSide = transform.position.y - transform.lossyScale.y / 2;
        rightSide = transform.position.x + transform.lossyScale.x / 2;
        leftSide = transform.position.x - transform.lossyScale.x / 2;
    }

    public void DestroyBrick()
    {
        UpdateManager.Instance.OnBrickDestruction(this);
    }
}
