using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool
{
    private int startingSpawnAmount;

    public Queue<GameBall> ballsOnReserve = new Queue<GameBall>();
    public List<GameBall> ballsInUse = new List<GameBall>();

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

    public GameBall CreateBall()
    {
        GameBall newBall = UpdateManager.Instance.SpawnBall();
        newBall.ToggleGameObject(false);
        ballsOnReserve.Enqueue(newBall);

        return newBall;
    }

    public GameBall GetBall()
    {
        GameBall ball;

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

    public void ReturnBall(GameBall ball)
    {
        if (ballsInUse.Contains(ball))
        {
            ballsInUse.Remove(ball);
            ball.ToggleGameObject(false);
            ballsOnReserve.Enqueue(ball);
        }
    }
}
