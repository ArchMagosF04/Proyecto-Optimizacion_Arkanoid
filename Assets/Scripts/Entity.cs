using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity
{
    protected Transform self;

    protected Vector3 direction;
    protected float speed;

    public Transform GetTransform()
    {
        return self;
    }
}
