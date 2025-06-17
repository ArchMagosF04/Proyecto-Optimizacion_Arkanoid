using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Ball : GameEntity
{
    #region Variables

    private float leftLimit;
    private float rightLimit;
    private float topLimit;
    private float bottomLimit;

    public bool isLaunched {  get; private set; }
    public bool isActive { get; private set; }

    private PlayerPaddle playerPaddle;

    private float hitBufferLength = 0f;
    private float hitBufferTimer;

    private SoundData ballCollision;
    private SoundData blockHit;

    #endregion

    public Ball(Transform transform, float speed, float leftLimit, float rightLimit, float topLimit, float bottomLimit, PlayerPaddle player, SoundData ballCollision, SoundData blockHit) 
    {
        this.Transform = transform;
        this.speed = speed;
        this.leftLimit = leftLimit;
        this.rightLimit = rightLimit;
        this.topLimit = topLimit;
        this.bottomLimit = bottomLimit;
        this.playerPaddle = player;
        this.ballCollision = ballCollision;
        this.blockHit = blockHit;

        isLaunched = false;
        isActive = false;
        Dimensions = new Vector2(transform.lossyScale.x / 2, transform.lossyScale.y / 2);
    }

    public void Initialize(Transform spawn)
    {
        isLaunched = false;
        Transform.position = spawn.position;
        Transform.parent = spawn.parent;
    }

    public void Update(float deltaTime, List<Brick> bricks)
    {
        MoveBall(deltaTime);

        if (isLaunched)
        {
            Vector3 nextPosition = Transform.position;
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

    #region Movement

    private void MoveBall(float deltaTime) //Updates the ball position.
    {
        if (isLaunched)
        {
            Transform.position += direction * (speed * deltaTime);
        }
    }

    public void LauchBall(Vector3 direction) //Launches the ball in the selected direction.
    {
        if (!isLaunched)
        {
            Transform.parent = null;
            this.direction = direction;
            isLaunched = true;
        }
    }

    #endregion

    #region Collisions

    private void BoundariesCollision(float deltaTime, Vector3 nextPosition) //Detects collision with the game walls.
    {
        if (nextPosition.x + Dimensions.x >= rightLimit || nextPosition.x - Dimensions.x <= leftLimit)
        {
            direction.x *= -1;
            UpdateManager.Instance.PlayAudioClip(ballCollision);
        }
        if (nextPosition.y + Dimensions.y >= topLimit)
        {
            direction.y *= -1;
            UpdateManager.Instance.PlayAudioClip(ballCollision);
        }
        if (Transform.position.y - Dimensions.y <= bottomLimit)
        {
            OnBallLost();
        }
    }

    private void BufferCountdown(float deltaTime) //Used to avoid the ball getting stuck during collisions.
    {
        if (hitBufferTimer > 0)
        {
            hitBufferTimer -= deltaTime;
        }
    }

   
    private void PaddleCollision(float deltaTime, Vector3 nextPosition) //Detects the collision with the paddle.
    {
        bool onSameX = false;

        float topSide = playerPaddle.Transform.position.y + playerPaddle.Dimensions.y;
        float bottomSide = playerPaddle.Transform.position.y - playerPaddle.Dimensions.y;
        float rightSide = playerPaddle.Transform.position.x + playerPaddle.Dimensions.x;
        float leftSide = playerPaddle.Transform.position.x - playerPaddle.Dimensions.x;

        if (nextPosition.x + Dimensions.x > leftSide && nextPosition.x - Dimensions.x < rightSide) { onSameX = true; }

        if (onSameX && nextPosition.y - Dimensions.y <= topSide && nextPosition.y >= bottomSide)
        {
            UpdateManager.Instance.IncreasePaddleHits();
            UpdateManager.Instance.PlayAudioClip(ballCollision);
            direction.y *= -1;

            float distanceFromCenter = Mathf.Abs(nextPosition.x - playerPaddle.Transform.position.x); //Depending on how far away from the center of the paddle the ball hits then it will be launched more in that direction.
            float horizontalAngle = (distanceFromCenter / 1.5f) * 0.6f;
            if (nextPosition.x < playerPaddle.Transform.position.x)
            {
                horizontalAngle *= -1;
            }

            direction.x = horizontalAngle;
            hitBufferTimer = hitBufferLength;
            return;
        }
    }

    private void BrickCollision(Vector3 nextPosition, List<Brick> bricks) //Detecs the collision with all the active bricks.
    {
        for (int i = 0; i < bricks.Count; i++)
        {
            if (bricks[i] != null) 
            {
                bool onSameX = false;
                bool onSameY = false;

                if (nextPosition.x > bricks[i].leftSide && nextPosition.x < bricks[i].rightSide) { onSameX = true; }

                if (nextPosition.y > bricks[i].bottomSide && nextPosition.y < bricks[i].topSide) { onSameY = true; }

                if (onSameX && (nextPosition.y - Dimensions.y <= bricks[i].topSide && nextPosition.y + Dimensions.y >= bricks[i].bottomSide))
                {
                    direction.y *= -1;
                    bricks[i].BrickHit();
                    hitBufferTimer = hitBufferLength;
                    UpdateManager.Instance.PlayAudioClip(blockHit);
                    return;
                }

                if (onSameY && (nextPosition.x + Dimensions.x >= bricks[i].leftSide && nextPosition.x - Dimensions.x <= bricks[i].rightSide))
                {
                    direction.x *= -1;
                    bricks[i].BrickHit();
                    hitBufferTimer = hitBufferLength;
                    UpdateManager.Instance.PlayAudioClip(blockHit);
                }

            }
        }
    }

    #endregion

    private void OnBallLost()
    {
        isLaunched = false;
        UpdateManager.Instance.OnBallDeath(this);
    }

    public void ToggleGameObject(bool input)
    {
        Transform.gameObject.SetActive(input);
        isActive = input;
    }
}
