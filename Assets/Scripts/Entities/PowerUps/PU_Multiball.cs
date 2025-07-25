using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PU_Multiball : GameEntity, IPowerUp
{
    private float bottomLimit;

    private bool isActive;

    private MultiballStats stats;

    private SoundData pickUpSound;
    public PU_Multiball(Transform transform, float speed, float bottomLimit, SoundData pickUpSound)
    {
        this.Transform = transform;
        this.speed = speed;
        this.bottomLimit = bottomLimit;
        this.pickUpSound = pickUpSound;
        isActive = true;
        direction.y = -1;
        stats = UpdateManager.Instance.gameSettings.multiballStats;
    }

    public void Activate(bool activate)
    {
        isActive = activate;
    }

    public void Update(float deltaTime, Transform paddle)
    {
        if (!isActive) return;

        Move(deltaTime);
        PowerUpLost();
        PaddleCollision(paddle);
    }

    private void Move(float deltaTime)
    {
        Transform.position += direction * (speed * deltaTime);
    }

    private void PowerUpLost()
    {
        if (Transform.position.y - Transform.lossyScale.y / 2 <= bottomLimit)
        {
            DestroyPowerUp();
        }
    }

    private void PaddleCollision(Transform paddle) //Checks if the power up collides with the paddle, and if so, it activates.
    {
        bool onSameX = false;

        float topSide = paddle.position.y + paddle.lossyScale.y / 2;
        float bottomSide = paddle.position.y - paddle.transform.lossyScale.y / 2;
        float rightSide = paddle.position.x + paddle.lossyScale.x / 2;
        float leftSide = paddle.position.x - paddle.transform.lossyScale.x / 2;

        if (Transform.position.x + Transform.lossyScale.x / 2 > leftSide && Transform.position.x - Transform.lossyScale.x / 2 < rightSide) { onSameX = true; }

        if (onSameX && Transform.position.y - Transform.lossyScale.y / 2 <= topSide && Transform.position.y >= bottomSide)
        {
            PowerUpEffect();
            DestroyPowerUp();
            return;
        }
    }

    public void PowerUpEffect() //Spawns two balls and immidiatly launches them.
    {
        UpdateManager.Instance.PlayAudioClip(pickUpSound);

        for (int i = 0; i < stats.BallAmount; i++)
        {
            UpdateManager.Instance.GetBallToLaunch();
            UpdateManager.Instance.LaunchBall();
        }
    }

    public void DestroyPowerUp() //Despawn the powerup.
    {
        UpdateManager.Instance.powerUpList.Remove(this);
        UpdateManager.Instance.DestroyGameObject(Transform.gameObject);
        isActive = false;
    }
}
