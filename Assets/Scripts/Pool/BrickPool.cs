using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickPool
{
    public Queue<Brick> bricksOnReserve = new Queue<Brick>(); //Contains the balls that are deactivated.
    public List<Brick> bricksInUse = new List<Brick>(); //Contains the balls that are in use.

    public BrickPool(int startAmount)
    {
        if (startAmount <= 0) return;
        for (int i = 0; i < startAmount; i++)
        {
            CreateBrick();
        }
    }

    public Brick CreateBrick() //Creates a new object.
    {
        Brick newBrick = UpdateManager.Instance.SpawnBrick();
        newBrick.ToggleGameObject(false);
        bricksOnReserve.Enqueue(newBrick);

        return newBrick;
    }

    public Brick GetBrick() //Gets a brick from the reserve queue, and if it's empty, then it creates a new one.
    {
        Brick brick;

        if (bricksOnReserve.Count > 0)
        {
            brick = bricksOnReserve.Dequeue();
        }
        else
        {
            brick = CreateBrick();
        }
        brick.ToggleGameObject(true);
        bricksInUse.Add(brick);
        return brick;
    }

    public void ReturnBrick(Brick brick) //Puts a brick from the inUse list to the reserve.
    {
        if (bricksInUse.Contains(brick))
        {
            bricksInUse.Remove(brick);
            brick.ToggleGameObject(false);
            bricksOnReserve.Enqueue(brick);
        }
    }

    public void ReturnAll() //Returns all activate bricks to the reserve queue.
    {
        for (int i = 0; i < bricksInUse.Count; i++)
        {
            bricksInUse[i].ToggleGameObject(false);
            bricksOnReserve.Enqueue(bricksInUse[i]);
        }

        bricksInUse.Clear();
    }
}
