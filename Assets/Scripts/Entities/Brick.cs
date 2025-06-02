using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class Brick : GameEntity
{
    #region Variables

    private int hitPoints;

    //Location of the brick's sides.
    public float topSide {  get; private set; }
    public float bottomSide { get; private set; }
    public float rightSide { get; private set; }
    public float leftSide { get; private set; }

    private MaterialPropertyBlock propertyBlock;

    #endregion

    public Brick(Transform transform, MaterialPropertyBlock propertyBlock)
    {
       this.Transform = transform;

        topSide = transform.position.y + transform.lossyScale.y / 2;
        bottomSide = transform.position.y - transform.lossyScale.y / 2;
        rightSide = transform.position.x + transform.lossyScale.x / 2;
        leftSide = transform.position.x - transform.lossyScale.x / 2;

        this.propertyBlock = propertyBlock;
    }

    public void BrickHit() //Executed when the ball collides with the brick.
    {
        hitPoints--;

        if (hitPoints <= 0)
        {
            DestroyBrick();
        }
    }

    public void DestroyBrick() 
    {

    }
}
