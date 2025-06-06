using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class Brick : GameEntity
{
    #region Variables

    private int hitPoints;
    private MeshRenderer mesh;
    private PowerUpHeld powerUp;

    //Location of the brick's sides.
    public float topSide {  get; private set; }
    public float bottomSide { get; private set; }
    public float rightSide { get; private set; }
    public float leftSide { get; private set; }

    #endregion

    public Brick(Transform transform)
    {
       this.Transform = transform;

        //CalculateBrickSidesPositions();

        mesh = transform.GetComponent<MeshRenderer>();
    }

    public void SetBrickType(int hp, Material material)
    {
        hitPoints = hp;
        mesh.material = material;
        CalculateBrickSidesPositions();
    }

    public void SetPowerUp(PowerUpHeld type)
    {
        powerUp = type;
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
        if (powerUp != PowerUpHeld.None) UpdateManager.Instance.SpawnPowerUp(Transform, powerUp);
        UpdateManager.Instance.OnBrickDeath(this);
    }

    private void CalculateBrickSidesPositions()
    {
        topSide = Transform.position.y + Transform.lossyScale.y / 2;
        bottomSide = Transform.position.y - Transform.lossyScale.y / 2;
        rightSide = Transform.position.x + Transform.lossyScale.x / 2;
        leftSide = Transform.position.x - Transform.lossyScale.x / 2;
    }

    public void ToggleGameObject(bool input)
    {
        Transform.gameObject.SetActive(input);
    }
}

public enum PowerUpHeld 
{
    None, Multiball, Speed
}
