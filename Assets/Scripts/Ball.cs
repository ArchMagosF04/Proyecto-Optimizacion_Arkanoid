using UnityEngine;
using System;

public class Ball : Entity
{
    float leftLimit, rightLimit, topLimit, bottomLimit;

    public Ball(float leftLimit, float rightLimit, float topLimit, float bottomLimit)
    {
        this.leftLimit = leftLimit;
        this.rightLimit = rightLimit;
        this.topLimit = topLimit;
        this.bottomLimit = bottomLimit;
    }

    public void Awake(Transform target)
    {
        this.self = target;
    }

    public void Update(float deltaTime)
    {
        self.position += direction * (speed * deltaTime);

        BorderCollisions();
    }

    public void LaunchBall(float speed, Vector3 direction)
    {
        this.speed = speed;
        this.direction = direction;
    }

    public void BorderCollisions()
    {
        if (self.position.x >= rightLimit || self.position.x <= leftLimit)
        {
            direction.x *= -1;
        }

        if (self.position.y >= topLimit)
        {
            direction.y *= -1;
        }

        if (self.position.y <= bottomLimit)
        {
            GameManager.Instance.DestroyBall(this, self);
            Debug.Log("Destroy ball");
        }
    }
}
