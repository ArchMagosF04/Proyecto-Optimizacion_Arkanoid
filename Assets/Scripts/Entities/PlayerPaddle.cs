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

    private float multiplierDuration = 7f;
    private float multiplierTimer;

    public PlayerPaddle(Transform transform, float speed, float leftLimit, float rightLimit)
    {
        this.transform = transform;
        this.speed = speed;
        this.screenLeftLimit = leftLimit;
        this.screenRightLimit = rightLimit;
    }

    public void Update(float deltaTime, float input)
    {
        MovePaddle(deltaTime, input);

        if (isSpeedPowerUpActive) { SpeedMultiplerTimer(deltaTime); }
    }

    private void MovePaddle(float deltaTime, float input) //Moves the paddle according to the received input value, while stopping at the left and right walls.
    {
        direction.x = input;

        if ((direction.x > 0 && transform.position.x + transform.lossyScale.x / 2 >= screenRightLimit) || (direction.x < 0 && transform.position.x - transform.lossyScale.x / 2 <= screenLeftLimit))
        {
            direction.x = 0f;
        }

        transform.position += direction * (speed * multiplier * deltaTime);
    }

    public void ToggleSpeedPowerUp(bool value) //Activates/Deactivates the bonus speed received from the speed powerup.
    {
        if (value) 
        {
            isSpeedPowerUpActive = true;
            multiplier = extraSpeed;
            multiplierTimer = multiplierDuration;
        }
        else
        {
            isSpeedPowerUpActive = false;
            multiplier = 1f;
        }
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
