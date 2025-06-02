using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class PlayerPaddle : GameEntity
{
    private float screenLeftLimit;
    private float screenRightLimit;

    private bool isSpeedPowerUpActive;

    private float multiplier = 1f;
    private float extraSpeed = 2f;

    private float multiplierDuration = 7f;
    private float multiplierTimer;

    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock materialPropertyBlock;

    private Color baseColor;
    private Color speedColor;

    public PlayerPaddle(Transform transform, float speed, float leftLimit, float rightLimit, MaterialPropertyBlock propertyBlock, Color baseColor, Color speedColor)
    {
        this.Transform = transform;
        this.speed = speed;
        this.screenLeftLimit = leftLimit;
        this.screenRightLimit = rightLimit;
        materialPropertyBlock = propertyBlock;

        meshRenderer = transform.GetComponent<MeshRenderer>();

        this.baseColor = baseColor;
        this.speedColor = speedColor;

        Dimensions = new Vector2(transform.lossyScale.x / 2, transform.lossyScale.y / 2);
    }

    public void Update(float deltaTime, float input)
    {
        MovePaddle(deltaTime, input);

        if (isSpeedPowerUpActive) { SpeedMultiplerTimer(deltaTime); }
    }

    private void MovePaddle(float deltaTime, float input) //Moves the paddle according to the received input value, while stopping at the left and right walls.
    {
        direction.x = input;

        if ((direction.x > 0 && Transform.position.x + Transform.lossyScale.x / 2 >= screenRightLimit) || (direction.x < 0 && Transform.position.x - Transform.lossyScale.x / 2 <= screenLeftLimit))
        {
            direction.x = 0f;
        }

        Transform.position += direction * (speed * multiplier * deltaTime);
    }

    public void ToggleSpeedPowerUp(bool value) //Activates/Deactivates the bonus speed received from the speed powerup.
    {
        Color color = Color.white;

        if (value) 
        {
            isSpeedPowerUpActive = true;
            multiplier = extraSpeed;
            multiplierTimer = multiplierDuration;

            color = speedColor;
        }
        else
        {
            isSpeedPowerUpActive = false;
            multiplier = 1f;
            color = baseColor;
        }

        meshRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetColor("_Color", color);
        meshRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    public void SpeedMultiplerTimer(float deltaTime) //Countdown of the speed powerup effect.
    {
        if (multiplierTimer > 0f)
        {
            multiplierTimer -= deltaTime;
        }
        else
        {
            ToggleSpeedPowerUp(false);
        }
    }
}
