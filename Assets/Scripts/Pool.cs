using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T>
{
    public List<T> objectsOnReserve = new List<T>();
    public List<T> objectsInUse = new List<T>();

    public T GetObject()
    {
        T newObject = default;

        if (objectsInUse.Count > 0)
        {
            newObject = objectsInUse[0];
            objectsInUse.RemoveAt(0);
        }

        objectsOnReserve.Add(newObject);
        return newObject;
    }

    public void RecycleObject(T newObject)
    {
        if (objectsOnReserve.Contains(newObject))
        {
            objectsOnReserve.Remove(newObject);
            objectsInUse.Add(newObject);
        }
    }
}
