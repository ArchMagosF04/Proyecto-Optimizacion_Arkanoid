using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PU_FastPaddle : GameEntity, IPowerUp
{
    private float bottomLimit;

    private bool isActive;

    public PU_FastPaddle(Transform transform, float speed, float bottomLimit)
    {
        this.Transform = transform;
        this.speed = speed;
        this.bottomLimit = bottomLimit;
        isActive = true;
        direction.y = -1;
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

    private void PaddleCollision(Transform paddle)
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

    public void PowerUpEffect()
    {
        UpdateManager.Instance.ActivateSpeedPowerUp();
    }

    public void DestroyPowerUp()
    {
        UpdateManager.Instance.powerUpList.Remove(this);
        UpdateManager.Instance.DestroyGameObject(Transform.gameObject);
        isActive = false;
    }
}

