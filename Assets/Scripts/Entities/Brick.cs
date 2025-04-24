using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : GameEntity
{
    #region Variables

    public enum PowerUpType { None, MultiBall, FastPaddle} 

    private PowerUpType heldPowerUp;

    //Location of the brick's sides.
    public float topSide {  get; private set; }
    public float bottomSide { get; private set; }
    public float rightSide { get; private set; }
    public float leftSide { get; private set; }

    #endregion

    public Brick(Transform transform)
    {
       this.transform = transform;

        topSide = transform.position.y + transform.lossyScale.y / 2;
        bottomSide = transform.position.y - transform.lossyScale.y / 2;
        rightSide = transform.position.x + transform.lossyScale.x / 2;
        leftSide = transform.position.x - transform.lossyScale.x / 2;
    }

    public void SetPowerUp(PowerUpType powerUp) //Called to select which power up the brick holds, or if it even has one.
    {
        heldPowerUp = powerUp;
    }

    public void DestroyBrick() //Executed when the ball collides with the brick.
    {
        if (heldPowerUp != PowerUpType.None) //If it holds a powerup then spawn it.
        {
            GameObject newObject = UpdateManager.Instance.SpawnPowerUp(transform.position);
            IPowerUp newPU = null;

            if (heldPowerUp == PowerUpType.MultiBall)
            {
                newPU = new PU_Multiball(newObject.transform, 10, UpdateManager.Instance.bottomLimit);
            }
            else if (heldPowerUp == PowerUpType.FastPaddle)
            {
                newPU = new PU_FastPaddle(newObject.transform, 10, UpdateManager.Instance.bottomLimit);
            }

            newPU.Activate(true);
            UpdateManager.Instance.powerUpList.Add(newPU);
        }

        UpdateManager.Instance.OnBrickDestruction(this);
    }
}
