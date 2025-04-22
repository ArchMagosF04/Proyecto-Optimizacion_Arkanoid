using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPaddle : GameEntity
{
    private float screenLeftLimit;
    private float screenRightLimit;

    private bool isSpeedPowerUpActive;

    private float multiplier = 1f;
    private float extraSpeed = 2f;

    public PlayerPaddle(Transform transform, float speed, float leftLimit, float rightLimit)
    {
        this.transform = transform;
        this.speed = speed;
        this.screenLeftLimit = leftLimit;
        this.screenRightLimit = rightLimit;
    }

    public void Initialize()
    {

    }

    public void Update(float deltaTime, float input)
    {
        MovePaddle(deltaTime, input);
    }

    private void MovePaddle(float deltaTime, float input)
    {
        direction.x = input;

        if ((direction.x > 0 && transform.position.x + transform.lossyScale.x / 2 >= screenRightLimit) || (direction.x < 0 && transform.position.x - transform.lossyScale.x / 2 <= screenLeftLimit))
        {
            direction.x = 0f;
        }

        transform.position += direction * (speed * multiplier * deltaTime);
    }

    public void ToggleSpeedPowerUp(bool value)
    {
        if (value) { multiplier = extraSpeed; }
        else
        {
            multiplier = 1f;
        }
    }
}
