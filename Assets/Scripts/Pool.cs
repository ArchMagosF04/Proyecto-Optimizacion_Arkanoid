using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool
{
    private int startingSpawnAmount;

    public Queue<Ball> ballsOnReserve = new Queue<Ball>();
    public List<Ball> ballsInUse = new List<Ball>();

    public Pool(int startingSpawnAmount)
    {
        this.startingSpawnAmount = startingSpawnAmount;
    }

    public void Initialize()
    {
        for (int i = 0; i < startingSpawnAmount; i++)
        {
            CreateBall();
        }
    }

    public Ball CreateBall()
    {
        Ball newBall = UpdateManager.Instance.SpawnBall();
        newBall.ToggleGameObject(false);
        ballsOnReserve.Enqueue(newBall);

        return newBall;
    }

    public Ball GetBall()
    {
        Ball ball;

        if (ballsOnReserve.Count > 0)
        {
            ball = ballsOnReserve.Dequeue(); 
        }
        else
        {
            ball = CreateBall();
        }
        ball.ToggleGameObject(true);
        ballsInUse.Add(ball);
        return ball;
    }

    public void ReturnBall(Ball ball)
    {
        if (ballsInUse.Contains(ball))
        {
            ballsInUse.Remove(ball);
            ball.ToggleGameObject(false);
            ballsOnReserve.Enqueue(ball);
        }
    }
}
