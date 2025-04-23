using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : GameEntity
{
    public enum PowerUpType { None, MultiBall, FastPaddle}

    public float topSide {  get; private set; }
    public float bottomSide { get; private set; }
    public float rightSide { get; private set; }
    public float leftSide { get; private set; }

    //private bool heldPowerUp;

    private PowerUpType heldPowerUp;

    public Brick(Transform transform, PowerUpType powerUp)
    {
       this.transform = transform;

        topSide = transform.position.y + transform.lossyScale.y / 2;
        bottomSide = transform.position.y - transform.lossyScale.y / 2;
        rightSide = transform.position.x + transform.lossyScale.x / 2;
        leftSide = transform.position.x - transform.lossyScale.x / 2;

        heldPowerUp = powerUp;
    }

    public void DestroyBrick()
    {
        if (heldPowerUp != PowerUpType.None)
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
