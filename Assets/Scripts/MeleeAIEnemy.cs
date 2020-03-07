using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAIEnemy : NavAgent
{
    public enum AIState
    {
        Attacking,
        NavigatingToPlayer,
        Dying,
        Dead,
        Despawning
    }
    AIState state;
    public Transform playerTrans;
    float ATTACK_RADIUS = 30.0f;
    int health = 100;
    float nextAttackTime;

    //All the ally AI that are pursuing this enemy
    List<MeleeAIAlly> pursuers = new List<MeleeAIAlly>();

    //How much more the enemies prefer attacking the player over allies
    static float PLAYER_PREFERENCE = 2.0f;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        nextAttackTime = Time.time;
        speed = 12.0f;
        state = AIState.NavigatingToPlayer;
    }

    public void ExecuteState()
    {
        //Attack player
        switch (state)
        {
            case AIState.Attacking:
                //Attack closest of the friendlies and player
                Transform closestAgent = playerTrans;
                float minDistance = Vector3.Distance(closestAgent.position, transform.position) / PLAYER_PREFERENCE;
                foreach(MeleeAIAlly ally in AIManager.Instance.allies)
                {
                    float dist = Vector3.Distance(ally.transform.position, transform.position);
                    if (dist < minDistance)
                    {
                        closestAgent = ally.transform;
                        minDistance = dist;
                    }
                }
                target = closestAgent;

                if ((playerTrans.position - transform.position).magnitude >= ATTACK_RADIUS)
                {
                    state = AIState.NavigatingToPlayer;
                    target = playerTrans;
                    
                }
                //If enemy does not have any pursuers, put it on the queue so that it can be assigned some
                else if(pursuers.Count == 0)
                {
                    AIManager.Instance.PutOnEnemyQueue(this);
                }
                break;
            case AIState.NavigatingToPlayer:
                if ((playerTrans.position - transform.position).magnitude < ATTACK_RADIUS)
                {
                    state = AIState.Attacking;
                }
                break;
            case AIState.Dying:
                //Play dying animation with coroutine maybe
                state = AIState.Despawning;
                break;
            case AIState.Dead:
                //In this state, the enemy can potentially be revived by the player. If we wait too long, the enemy
                //will despawn
                break;
            case AIState.Despawning:
                AIManager.Instance.agents.Remove(this);
                //Removing from enemies list will prevent this enemy from having its state executed again
                AIManager.Instance.enemies.Remove(this);
                Destroy(this.gameObject);
                break;
        }
    }


    public override void MoveAgent(Vector3 heading)
    {
        transform.Translate(heading * Time.deltaTime, Space.World);
        Vector3 curPos = new Vector3(transform.position.x, 0.1f, transform.position.z);
        //desiredHeading = TARGET_SPEED * (pathPoints[0] - curPos).normalized;
        //Smooth movement
        desiredHeading = Vector3.Lerp(heading, speed * (pathPoints[0] - curPos).normalized, 5.0f * Time.deltaTime);
        Debug.DrawLine(curPos, curPos + desiredHeading, Color.cyan);

    }

    public void AddPursuer(MeleeAIAlly ally)
    {
        pursuers.Add(ally);
    }

    public void RemovePursuer(MeleeAIAlly ally)
    {
        pursuers.Remove(ally);
    }

    private void OnDestroy()
    {
        
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if(health <= 0)
        {
            state = AIState.Dying;

            //Disable collider to avoid future triggers
            gameObject.GetComponent<Collider>().enabled = false;
            foreach (MeleeAIAlly friendly in pursuers)
            {
                friendly.TargetWasKilled();
            }
        }
    }

    
    //This gets called before Update functions.
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<MeleeAIAlly>() && Time.time > nextAttackTime)
        {
            //Attack ally AI
            MeleeAIAlly allyAI = other.gameObject.GetComponent<MeleeAIAlly>();
            nextAttackTime = Time.time + Random.Range(0.5f, 1.0f);
            allyAI.TakeDamage(20);
        }
    }
}
