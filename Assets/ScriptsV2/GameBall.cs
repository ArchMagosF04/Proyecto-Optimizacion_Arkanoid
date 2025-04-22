using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBall : GameEntity
{
    private float leftLimit;
    private float rightLimit;
    private float topLimit;
    private float bottomLimit;

    public bool isLaunched {  get; private set; }
    public bool isActive { get; private set; }

    private PlayerPaddle playerPaddle;

    private float radius;

    private float hitBufferLength = 0.1f;
    private float hitBufferTimer;

    public GameBall(Transform transform, float speed, float leftLimit, float rightLimit, float topLimit, float bottomLimit, PlayerPaddle player) 
    {
        this.transform = transform;
        this.speed = speed;
        this.leftLimit = leftLimit;
        this.rightLimit = rightLimit;
        this.topLimit = topLimit;
        this.bottomLimit = bottomLimit;
        this.playerPaddle = player;

        isLaunched = false;
        isActive = false;
        radius = transform.lossyScale.x / 2;
    }

    public void Initialize(Transform spawn)
    {
        isLaunched = false;
        transform.position = spawn.position;
    }

    public void Update(float deltaTime, List<Brick> bricks)
    {
        MoveBall(deltaTime);

        if (isLaunched)
        {
            Vector3 nextPosition = transform.position;
            nextPosition += direction * (speed * deltaTime);

            BoundariesCollision(deltaTime, nextPosition);
            if (direction.y < 0 && hitBufferTimer <= 0)
            {
                PaddleCollision(deltaTime, nextPosition);
            }

            //if (hitBufferTimer <= 0) BricksCollision(bricks);

            BufferCountdown(deltaTime);
        }
    }

    private void MoveBall(float deltaTime)
    {
        if (isLaunched)
        {
            transform.position += direction * (speed * deltaTime);
        }
        else
        {
            transform.position = new Vector3(playerPaddle.transform.position.x, transform.position.y, transform.position.z);
        }
    }

    public void LauchBall(Vector3 direction)
    {
        if (!isLaunched)
        {
            this.direction = direction;
            isLaunched = true;
        }
    }

    private void BoundariesCollision(float deltaTime, Vector3 nextPosition)
    {
        if (nextPosition.x + radius >= rightLimit || nextPosition.x - radius <= leftLimit)
        {
            direction.x *= -1;
        }
        if (nextPosition.y + radius >= topLimit)
        {
            direction.y *= -1;
        }
        if (transform.position.y - radius <= bottomLimit)
        {
            OnBallLost();
        }
    }

    private void BufferCountdown(float deltaTime)
    {
        if (hitBufferTimer > 0)
        {
            hitBufferTimer -= deltaTime;
        }
    }

   
    private void PaddleCollision(float deltaTime, Vector3 nextPosition)
    {
        bool onSameX = false;

        float topSide = playerPaddle.transform.position.y + playerPaddle.transform.lossyScale.y / 2;
        float bottomSide = playerPaddle.transform.position.y - playerPaddle.transform.lossyScale.y / 2;
        float rightSide = playerPaddle.transform.position.x + playerPaddle.transform.lossyScale.x / 2;
        float leftSide = playerPaddle.transform.position.x - playerPaddle.transform.lossyScale.x / 2;

        if (nextPosition.x > leftSide && nextPosition.x < rightSide) { onSameX = true; }

        if (onSameX && nextPosition.y - radius <= topSide && nextPosition.y >= bottomSide)
        {
            direction.y *= -1;
            hitBufferTimer = hitBufferLength;
            Debug.Log("paddle ");
            return;
        }
    }

        private void BrickCollision(Vector3 nextPosition, List<Brick> bricks)
    {
        for (int i = 0; i < bricks.Count; i++)
        {
            if (bricks[i] != null) 
            {
                bool onSameX = false;
                bool onSameY = false;

                if (nextPosition.x > bricks[i].leftSide && nextPosition.x < bricks[i].rightSide) { onSameX = true; }

                if (nextPosition.y > bricks[i].bottomSide && nextPosition.y < bricks[i].topSide) { onSameY = true; }

                if (onSameX && (nextPosition.y - radius <= bricks[i].topSide || nextPosition.y + radius >= bricks[i].bottomSide))
                {
                    direction.y *= -1;
                    bricks[i].DestroyBrick();
                    hitBufferTimer = hitBufferLength;
                    return;
                }

                if (onSameY && (nextPosition.x + radius >= bricks[i].leftSide || nextPosition.x - radius <= bricks[i].rightSide))
                {
                    direction.x *= -1;
                    bricks[i].DestroyBrick();
                    hitBufferTimer = hitBufferLength;
                }
                Debug.Log("brick ");
            }
        }
    }

    public void BricksCollision(List<Brick> bricks)
    {
        foreach (Brick brick in bricks) //calculetes the collision for each brick in the world 
        {
            if (brick == null) return;

            bool onSameColumn = false;
            bool onSameRow = false;

            float leftSide = brick.transform.position.x - (1.75f - 0.3f);
            float rightSide = brick.transform.position.x + (1.75f + 0.3f);
            float upperSide = brick.transform.position.y + 1;
            float lowerSide = brick.transform.position.y - 1;

            if (transform.position.x <= rightSide && transform.position.x >= leftSide)
            {
                onSameColumn = true;
            }
            if (transform.position.y <= upperSide && transform.position.y >= lowerSide)
            {
                onSameRow = true;
            }

            if (circleRect(transform.position.x, transform.position.y, 0.5f, brick.transform.position.x, brick.transform.position.y, brick.transform.lossyScale.x, brick.transform.lossyScale.y) && hitBufferTimer <= 0)
            {
                if (onSameColumn)
                {
                    direction.y *= -1;
                    hitBufferTimer = hitBufferLength;
                    bricks.Remove(brick);
                    brick.DestroyBrick();
                    return; // exit if was a collision
                }
                if (onSameRow)
                {
                    direction.x *= -1;
                    hitBufferTimer = hitBufferLength;
                    bricks.Remove(brick);
                    brick.DestroyBrick();
                    return; // exit if was a collision
                }

            }

        }
    }

    private void OnBallLost()
    {
        Debug.Log("Ball lost");
        isLaunched = false;
        UpdateManager.Instance.OnBallDeath(this);
    }

    public void ToggleGameObject(bool input)
    {
        transform.gameObject.SetActive(input);
    }

    public bool circleRect(float cx, float cy, float radius, float rx, float ry, float rw, float rh)
    {

        // temporary variables to set edges for testing
        float testX = cx;
        float testY = cy;

        // which edge is closest?
        if (cx < rx - rw / 2) testX = rx;      // test left edge
        else if (cx > rx + rw / 2) testX = rx + rw;   // right edge
        if (cy < ry - rh / 2) testY = ry;      // top edge
        else if (cy > ry + rh / 2) testY = ry + rh;   // bottom edge

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
    private bool intersects(Transform rect)
    {
        float disX = Mathf.Abs(transform.position.x - rect.position.x);
        float disY = Mathf.Abs(transform.position.y - rect.position.y);

        if (disX > (rect.lossyScale.x / 2 + transform.lossyScale.x / 2) || (disY > (rect.lossyScale.y / 2 + transform.lossyScale.y / 2))) 
            { return false; }
        //if (disY > (rect.lossyScale.y / 2 + transform.lossyScale.y / 2)) { return false; }

        if (disX <= (rect.lossyScale.x / 2)) 
        {
            direction.x *= -1;
            hitBufferTimer = hitBufferLength;
            Debug.Log("hitX");
            return true; 
        }
        if (disY <= (rect.lossyScale.y / 2)) 
        {
            direction.y *= -1;
            hitBufferTimer = hitBufferLength;
            Debug.Log("hitY");
            return true; 
        }

        float cornerDistance = Mathf.Pow(disX - rect.lossyScale.x / 2,2) + Mathf.Pow(disY - rect.lossyScale.y / 2,2);

        if (cornerDistance <= Mathf.Pow(transform.lossyScale.x / 2, 2))
        {
            direction *= -1;
            hitBufferTimer = hitBufferLength;
            Debug.Log("hit");
            return true;
        }
        else 
        { 
            return false;
        }

    }
}
