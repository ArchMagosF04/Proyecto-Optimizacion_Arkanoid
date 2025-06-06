using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPool
{
    private int startingSpawnAmount;

    public Queue<Ball> ballsOnReserve = new Queue<Ball>(); //Contains the balls that are deactivated.
    public List<Ball> ballsInUse = new List<Ball>(); //Contains the balls that are in use.

    public BallPool(int startingSpawnAmount)
    {
        this.startingSpawnAmount = startingSpawnAmount;
        Initialize();
    }

    public void Initialize() //It will instantiate how many items are set in the startingSpawnAmount.
    {
        if (startingSpawnAmount <= 0) return;
        for (int i = 0; i < startingSpawnAmount; i++)
        {
            CreateBall();
        }
    }

    public void CreateBall() //Creates a new object.
    {
        Ball newBall = UpdateManager.Instance.SpawnBall();
        newBall.ToggleGameObject(false);
        ballsOnReserve.Enqueue(newBall);
    }

    public Ball GetBall() //Gets a ball from the reserve queue, and if it's empty, then it creates a new one.
    {
        Ball ball;

        if (ballsOnReserve.Count == 0)
        {
            CreateBall();
            
        }

        ball = ballsOnReserve.Dequeue();

        ball.ToggleGameObject(true);
        ballsInUse.Add(ball);
        return ball;
    }

    public void ReturnBall(Ball ball) //Puts a ball from the inUse list to the reserve.
    {
        if (ballsInUse.Contains(ball))
        {
            ballsInUse.Remove(ball);
            ball.ToggleGameObject(false);
            ballsOnReserve.Enqueue(ball);
        }
    }

    public void ReturnAll() //Returns all activate balls to the reserve queue.
    {
        for(int i = 0; i < ballsInUse.Count; i++)
        {
            ballsInUse[i].ToggleGameObject(false);
            ballsOnReserve.Enqueue(ballsInUse[i]);
        }

        ballsInUse.Clear();
    }
}
