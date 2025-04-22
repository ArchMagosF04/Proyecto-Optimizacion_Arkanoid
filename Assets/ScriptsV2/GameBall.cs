using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

            if (hitBufferTimer <= 0) BrickCollision(nextPosition ,bricks);

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

        if (nextPosition.x + radius > leftSide && nextPosition.x - radius < rightSide) { onSameX = true; }

        if (onSameX && nextPosition.y - radius <= topSide && nextPosition.y >= bottomSide)
        {
            direction.y *= -1;

            float distanceFromCenter = Mathf.Abs(nextPosition.x - playerPaddle.transform.position.x);
            float horizontalAngle = (distanceFromCenter / 1.5f) * 0.6f;
            if (nextPosition.x < playerPaddle.transform.position.x)
            {
                horizontalAngle *= -1;
            }

            direction.x = horizontalAngle;
            hitBufferTimer = hitBufferLength;
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

                if (onSameX && (nextPosition.y - radius <= bricks[i].topSide && nextPosition.y + radius >= bricks[i].bottomSide))
                {
                    direction.y *= -1;
                    bricks[i].DestroyBrick();
                    hitBufferTimer = hitBufferLength;
                    return;
                }

                if (onSameY && (nextPosition.x + radius >= bricks[i].leftSide && nextPosition.x - radius <= bricks[i].rightSide))
                {
                    direction.x *= -1;
                    bricks[i].DestroyBrick();
                    hitBufferTimer = hitBufferLength;
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
}
