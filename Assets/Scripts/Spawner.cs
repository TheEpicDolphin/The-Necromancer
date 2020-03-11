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
            MeleeAIEnemy enemyAI = (MeleeAIEnemy)Instantiate(Resources.Load("Prefabs/Enemy", typeof(MeleeAIEnemy)));
            enemyAI.transform.position = transform.position;
            enemyAI.target = AIManager.Instance.playerAgent.transform;
            enemyAI.playerTrans = AIManager.Instance.playerAgent.transform;
        }
        
    }
}
