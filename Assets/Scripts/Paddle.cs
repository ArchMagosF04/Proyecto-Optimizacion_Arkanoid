using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Paddle : Entity
{
    private int Xdir;
    Transform rootedBall;
    float screenLeftLimit, screenRightLimit;

    public float leftSide, rightSide, upperSide, lowerSide;

    public Paddle(float speed, float leftLimit, float rightLimit)
    {
        this.speed = speed;
        this.screenLeftLimit = leftLimit;
        this.screenRightLimit = rightLimit;
    }

    public void Awake(Transform target)
    {
        this.self = target;

        //leftSide = self.position.x - self.localScale.x / 2;
        //rightSide = self.position.x + self.localScale.x / 2;
        //upperSide = self.position.y + self.localScale.y / 2;
        //lowerSide = self.position.y - self.localScale.y / 2;
    }

    public void SetBall(Transform ball)
    {
        rootedBall = ball;
    }

    public void UnbindBall()
    {
        rootedBall = null;
    }

    public void Update(float deltaTime, float xInput)
    {
        direction.x = xInput;

        if ((direction.x > 0 && self.position.x + self.localScale.x / 2 >= screenRightLimit) || (direction.x < 0 && self.position.x - self.localScale.x / 2 <= screenLeftLimit))
        {
            direction.x = 0f;
        }
        
        self.position += direction * (speed * deltaTime);

        if (rootedBall != null)
        {
            rootedBall.position = new Vector3(self.position.x, rootedBall.position.y, rootedBall.position.z);
        }
    }
}
