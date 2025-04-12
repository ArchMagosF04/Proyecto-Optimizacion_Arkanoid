using UnityEngine;
using System;
using static UnityEngine.GraphicsBuffer;

public class Ball : Entity
{
    float leftLimit, rightLimit, topLimit, bottomLimit;

    private bool hasLaunched = false;

    Collider2D[] collider2Ds;

    float collisionTimer = 0f;
    bool hitOccured = false;

    private bool isActive = false;
    public bool IsActive => isActive;

    public Ball(Transform target, float leftLimit, float rightLimit, float topLimit, float bottomLimit)
    {
        this.self = target;

        this.leftLimit = leftLimit;
        this.rightLimit = rightLimit;
        this.topLimit = topLimit;
        this.bottomLimit = bottomLimit;

        collider2Ds = new Collider2D[2];
    }

    public void Awake()
    {
        hasLaunched = false;
        hitOccured = false;
    }

    public void Update(float deltaTime, Transform paddle)
    {
        self.position += direction * (speed * deltaTime);

        BorderCollisions();
        PaddleCollision(paddle);
        TimerCount(deltaTime);
    }

    private void TimerCount(float deltaTime)
    {
        if (hitOccured)
        {
            collisionTimer -= deltaTime;
            if (collisionTimer <= 0f)
            {
                hitOccured = false;
            }
        }
    }

    private void OnHit()
    {
        hitOccured = true;
        collisionTimer = 0.2f;
    }

    public void LaunchBall(float speed, Vector3 direction)
    {
        if (!hasLaunched)
        {
            this.speed = speed;
            this.direction = direction;
            hasLaunched = true;
        }
    }

    public void BorderCollisions()
    {
        if ((self.position.x >= rightLimit || self.position.x <= leftLimit) && !hitOccured)
        {
            direction.x *= -1;
            OnHit();
        }

        if (self.position.y >= topLimit && !hitOccured)
        {
            direction.y *= -1;
            OnHit();
        }

        if (self.position.y <= bottomLimit)
        {
            GameManager.Instance.ReturnBallToPool(this);
        }
    }

    public void PaddleCollision(Transform paddle)
    {
        int hits = Physics2D.OverlapCircleNonAlloc(self.position, 0.55f, collider2Ds);

        if (self.position.x > paddle.position.x - paddle.localScale.x/2 && self.position.x < paddle.position.x + paddle.localScale.x / 2)
        {
            if (hits > 1 && !hitOccured)
            {
                direction.y *= -1;
                OnHit();
            }
        }
        else if (self.position.y < paddle.position.y + paddle.localScale.y / 2) 
        {
            if (hits > 1 && !hitOccured)
            {
                direction.x *= -1;
                OnHit();
            }
        }
    }

    public void ActivateBall(bool input)
    {
        self.gameObject.SetActive(input);
        isActive = input;

        if (!input)
        {
            this.speed = 0;
            this.direction = Vector3.zero;
        }
    }
}
