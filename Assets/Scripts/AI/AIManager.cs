using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    private static AIManager _instance;
    public static AIManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public List<NavAgent> agents = new List<NavAgent>();
    public List<MeleeAIAlly> allies;
    public List<MeleeAIEnemy> enemies;
    public PlayerMovementController playerAgent;

    LinkedList<MeleeAIEnemy> enemyQueue;
    List<MeleeAIAlly> readyForEnemy;
    
    // Start is called before the first frame update
    void Start()
    {
        playerAgent = transform.parent.gameObject.GetComponent<PlayerMovementController>();
    }

    // Update is called once per frame
    void Update()
    {
        readyForEnemy = new List<MeleeAIAlly>();
        MeleeAIAlly[] alliesCopy = new MeleeAIAlly[allies.Count];
        allies.CopyTo(alliesCopy);
        //Execute ally states
        foreach (MeleeAIAlly ally in alliesCopy)
        {
            ally.ExecuteState();
        }

        enemyQueue = new LinkedList<MeleeAIEnemy>();
        MeleeAIEnemy[] enemiesCopy = new MeleeAIEnemy[enemies.Count];
        enemies.CopyTo(enemiesCopy);
        //Execute enemy states
        foreach (MeleeAIEnemy enemy in enemiesCopy)
        {
            enemy.ExecuteState();
        }

        LinkedListNode<MeleeAIEnemy> enemyHead = enemyQueue.First;
        while (enemyHead != null && readyForEnemy.Count > 0)
        {
            //Sort to get closest allies to enemy
            readyForEnemy.Sort((f1, f2) =>
                -Vector3.Distance(f1.transform.position, enemyHead.Value.transform.position).CompareTo(Vector3.Distance(f2.transform.position, enemyHead.Value.transform.position)));

            readyForEnemy[readyForEnemy.Count - 1].AttackEnemy(enemyHead.Value);
            enemyHead.Value.AddPursuer(readyForEnemy[readyForEnemy.Count - 1]);
            readyForEnemy.RemoveAt(readyForEnemy.Count - 1);
            enemyQueue.RemoveFirst();
            enemyHead = enemyHead.Next;
        }

    }


    public void PutOnDispatchQueue(MeleeAIAlly allyAI)
    {
        readyForEnemy.Add(allyAI);
    }

    public void PutOnEnemyQueue(MeleeAIEnemy enemyAI)
    {
        enemyQueue.AddLast(enemyAI);
    }

}
