using UnityEngine;

public class GameEntity 
{
    public Transform Transform { get; protected set; }
    public Vector2 Dimensions { get; protected set; }

    protected Vector3 direction;

    protected float speed;
}
