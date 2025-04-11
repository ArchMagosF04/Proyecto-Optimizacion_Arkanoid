using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Paddle : Entity
{
    private int Xdir;
    Transform rootedBall;

    public Paddle(float speed)
    {
        this.speed = speed;
    }

    public void Awake(Transform target)
    {
        this.self = target;
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

        self.position += direction * (speed * deltaTime);

        if (rootedBall != null)
        {
            rootedBall.position = new Vector3(self.position.x, rootedBall.position.y, rootedBall.position.z);
        }
    }
}
