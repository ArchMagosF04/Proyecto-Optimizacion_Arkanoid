using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPowerUp
{
    public void Activate(bool input);

    public void PowerUpEffect();

    public void Update(float deltaTime, Transform paddle);

    public void DestroyPowerUp();
}
