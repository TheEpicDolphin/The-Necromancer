using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class BoidsMovement : MonoBehaviour
{
    void LateUpdate()
    {
        List<Zombie> zombies = AIManager.Instance.zombies;
        foreach(Zombie zombie in zombies)
        {
            zombie.MovementUpdate();
        }
    }   
}

