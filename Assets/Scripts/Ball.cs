using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

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

    public void Update(float deltaTime, Transform paddle, List<Transform> bricks)
    {
        self.position += direction * (speed * deltaTime);

        BorderCollisions();
        PaddleCollision(paddle);
        BricksCollision(bricks);
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

    //public void BlockCollision()
    //{
    //    int hits = Physics2D.OverlapCircleNonAlloc(self.position, 0.55f, collider2Ds);

    //}

    //public void PaddleCollisionV2(Transform paddle, Paddle entity)
    //{
    //    float leftSide = paddle.position.x - paddle.localScale.x / 2;
    //    float rightSide = paddle.position.x + paddle.localScale.x / 2;
    //    float upperSide = paddle.position.y + paddle.localScale.y / 2;
    //    float lowerSide = paddle.position.y - paddle.localScale.y / 2;


    //    Vector3 closestPoint = new Vector3(
    //        Mathf.Max(paddle.position.x - paddle.localScale.x / 2, Mathf.Min(self.position.x, paddle.position.x + paddle.localScale.x / 2)),
    //        Mathf.Max(paddle.position.y - paddle.localScale.y / 2, Mathf.Min(self.position.y, paddle.position.y + paddle.localScale.y / 2)),
    //        0
    //    );

    //    float distance = Vector3.Distance(self.position, closestPoint);

    //    //if (distance <= self.position.x / 2)
    //    //{
    //    //    Vector3 penetrationDir = (self.position - closestPoint).normalized;
    //    //    if (Mathf.Abs(penetrationDir.x) > Mathf.Abs(penetrationDir.y) && !hitOccured)
    //    //    {
    //    //        direction.y *= -1;
    //    //        OnHit();
    //    //    }
    //    //    else if (Mathf.Abs(penetrationDir.x) < Mathf.Abs(penetrationDir.y) && !hitOccured)
    //    //    {
    //    //        direction.x *= -1;
    //    //        OnHit();
    //    //    }
    //    //}

    //    if (distance <= self.position.x / 2)
    //    {
    //        if (self.position.x <= rightSide && self.position.x >= leftSide && !hitOccured)
    //        {
    //            direction.y *= -1;
    //            OnHit();
    //        }
    //        else if (self.position.y <= upperSide && self.position.y >= lowerSide && !hitOccured)
    //        {
    //            direction.x *= -1;
    //            OnHit();
    //        }
    //    }
    //}

    public void PaddleCollisionV3(Transform paddle)
    {
        //float leftSide = paddle.position.x - paddle.localScale.x / 2;
        //float rightSide = paddle.position.x + paddle.localScale.x / 2;
        //float upperSide = paddle.position.y + paddle.localScale.y / 2;
        //float lowerSide = paddle.position.y - paddle.localScale.y / 2;

        bool onSameColumn = false;
        bool onSameRow = false;

        float leftSide = paddle.position.x - (1.75f - 0.3f);
        float rightSide = paddle.position.x + (1.75f + 0.3f);
        float upperSide = paddle.position.y + 1;
        float lowerSide = paddle.position.y - 1;

        if (self.position.x <= rightSide && self.position.x >= leftSide)
        {
            onSameColumn = true;
        }
        if (self.position.y <= upperSide && self.position.y >= lowerSide)
        {
            onSameRow = true;
        }

        if (circleRect(self.position.x, self.position.y, 0.5f,  paddle.position.x, paddle.position.y, paddle.lossyScale.x, paddle.lossyScale.y) && !hitOccured)
        {
            if (onSameColumn)
            {
                direction.y *= -1;
                OnHit();
                return;
            }
            if (onSameRow)
            {
                if ((direction.x > 0 && paddle.position.x < self.position.x) || (direction.x < 0 && paddle.position.x > self.position.x))
                {
                    direction.x *= 2;
                    OnHit();
                }
                else
                {
                    direction.x *= -1;
                    OnHit();
                }
            }
        }
    }

    public void BricksCollision(List<Transform> bricks)
    {
        foreach (Transform brick in bricks)
        {
            bool onSameColumn = false;
            bool onSameRow = false;

            float leftSide = brick.position.x - (1.75f - 0.3f);
            float rightSide = brick.position.x + (1.75f + 0.3f);
            float upperSide = brick.position.y + 1;
            float lowerSide = brick.position.y - 1;

            if (self.position.x <= rightSide && self.position.x >= leftSide)
            {
                onSameColumn = true;
            }
            if (self.position.y <= upperSide && self.position.y >= lowerSide)
            {
                onSameRow = true;
            }

            if (circleRect(self.position.x, self.position.y, 0.5f, brick.position.x, brick.position.y, brick.lossyScale.x, brick.lossyScale.y) && !hitOccured)
            {
                if (onSameColumn)
                {
                    direction.y *= -1;
                    OnHit();
                    bricks.Remove(brick);
                    GameManager.Instance.DisableBlock(brick);
                    return; // Salimos si hubo una colisión
                }
                if (onSameRow)
                {
                    if ((direction.x > 0 && brick.position.x < self.position.x) || (direction.x < 0 && brick.position.x > self.position.x))
                    {
                        direction.x *= 2;
                        OnHit();
                    }
                    else
                    {
                        direction.x *= -1;
                        OnHit();
                    }
                    bricks.Remove(brick);
                    GameManager.Instance.DisableBlock(brick);
                    return; // Salimos si hubo una colisión
                }

            }

        }
    }

    public bool circleRect(float cx, float cy, float radius, float rx, float ry, float rw, float rh)
    {

        // temporary variables to set edges for testing
        float testX = cx;
        float testY = cy;

        // which edge is closest?
        if (cx < rx - rw/2) testX = rx;      // test left edge
        else if (cx > rx + rw/2) testX = rx + rw;   // right edge
        if (cy < ry -rh/2) testY = ry;      // top edge
        else if (cy > ry + rh/2) testY = ry + rh;   // bottom edge

        // get distance from closest edges
        float distX = cx - testX;
        float distY = cy - testY;
        float distance = Mathf.Sqrt((distX * distX) + (distY * distY));

        // if the distance is less than the radius, collision!
        if (distance <= radius)
        {
            return true;
        }
        return false;
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
