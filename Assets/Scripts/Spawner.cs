using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    float coolDown = 10.0f;
    float nextSpawnTime;
    // Start is called before the first frame update
    void Start()
    {
        nextSpawnTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Time.time > nextSpawnTime)
        {
            nextSpawnTime = Time.time + coolDown;
            Zombie zombieAI = (Zombie)Instantiate(Resources.Load("Prefabs/Zombie", typeof(Zombie)));
            zombieAI.transform.position = transform.position;
            zombieAI.target = AIManager.Instance.playerAgent.transform;
        }
        
    }
}
