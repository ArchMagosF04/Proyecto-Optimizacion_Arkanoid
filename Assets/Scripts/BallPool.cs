using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPool
{
    private int startingCount = 3;

    public Queue<Ball> objectsOnReserve = new Queue<Ball>();
    //public List<Ball> objectsInUse = new List<Ball>();

    public BallPool(int count) 
    {
        startingCount = count;
    }

    public void InitializePool()
    {
        for(int i = 0; i < startingCount; i++)
        {
            GameObject newBall = GameManager.Instance.SpawnBall();

            Ball ball = new Ball(newBall.transform ,GameManager.Instance.leftLimit, GameManager.Instance.rightLimit, GameManager.Instance.topLimit, GameManager.Instance.bottomLimit);
            ball.Awake();
            ball.ActivateBall(false);
            objectsOnReserve.Enqueue(ball);
        }
    }

    public Ball GetBall()
    {
        Ball newBall = objectsOnReserve.Dequeue();
        newBall.ActivateBall(true);
        return newBall;
    }

    public void ReturnBall(Ball ball)
    {
        ball.ActivateBall(false);
        objectsOnReserve.Enqueue(ball);
    }

    //public Ball GetObject()
    //{
    //    Ball newObject = default;

    //    if (objectsOnReserve.Count > 0)
    //    {
    //        newObject = objectsOnReserve.Pop();
    //        objectsInUse.Add(newObject);
    //    }

    //    return newObject;
    //}

    //public void RecycleObject(Ball newObject)
    //{
    //    if (objectsInUse.Contains(newObject))
    //    {
    //        objectsInUse.Remove(newObject);
    //        objectsOnReserve.Push(newObject);
    //    }
    //}
}
