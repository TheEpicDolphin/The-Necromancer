using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MeleeAIAlly : NavAgent
{
    public enum AIState
    {
        AttackingEnemy,
        ReturningToMaster,
        SearchingForEnemy,
        Dying,
        Despawning
    }
    AIState state;
    public Transform master;
    Transform wanderingTransform;

    //The enemy AI that this ally is targeting
    MeleeAIEnemy enemyTarget;
    float nextWanderTime;
    float WANDER_RADIUS = 30.0f;
    int health = 150;
    float nextAttackTime;
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        nextWanderTime = Time.time;
        nextAttackTime = Time.time;
        state = AIState.ReturningToMaster;

        GameObject go = new GameObject();
        go.transform.position = transform.position;
        wanderingTransform = go.transform;
        wanderingTransform.parent = master;

        speed = 15.0f;
        target = master;
    }

    public void ExecuteState()
    {
        
        switch (state)
        {
            case AIState.AttackingEnemy:
                if ((master.position - transform.position).magnitude >= WANDER_RADIUS)
                {
                    state = AIState.ReturningToMaster;
                    target = master;
                    enemyTarget.RemovePursuer(this);
                    enemyTarget = null;
                }
                break;
            case AIState.ReturningToMaster:
                if ((master.position - transform.position).magnitude < WANDER_RADIUS)
                {
                    state = AIState.SearchingForEnemy;
                    target = wanderingTransform;
                }
                break;
            case AIState.SearchingForEnemy:
                if ((master.position - transform.position).magnitude >= WANDER_RADIUS)
                {
                    state = AIState.ReturningToMaster;
                    target = master;
                }
                else
                {
                    AIManager.Instance.PutOnDispatchQueue(this);
                    if (Time.time > nextWanderTime)
                    {
                        nextWanderTime = Time.time + Random.Range(3.0f, 8.0f);
                        Vector2 randPos2d = WANDER_RADIUS * Random.insideUnitCircle;
                        target.position = master.position + new Vector3(randPos2d.x, 0.0f, randPos2d.y);
                    }
                }
                break;
            case AIState.Dying:
                //Play dying animation with coroutine maybe
                state = AIState.Despawning;
                break;
            case AIState.Despawning:
                AIManager.Instance.agents.Remove(this);
                //Removing from enemies list will prevent this enemy from having its state executed again
                AIManager.Instance.allies.Remove(this);
                Destroy(this.gameObject);
                break;
        }
    }

    public void AttackEnemy(MeleeAIEnemy enemyAI)
    {
        state = AIState.AttackingEnemy;
        enemyTarget = enemyAI;
        target = enemyAI.transform;
    }

    public void TargetWasKilled()
    {
        state = AIState.SearchingForEnemy;
        enemyTarget = null;
        target = wanderingTransform;
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

    public void TakeDamage(int damage)
    {
        //Invincible for now
        //health -= damage;
        if (health <= 0)
        {
            state = AIState.Dying;

            //Disable collider to avoid future triggers
            gameObject.GetComponent<Collider>().enabled = false;

            enemyTarget.RemovePursuer(this);
            enemyTarget = null;
        }
    }

    private void OnDestroy()
    {
        if (wanderingTransform)
        {
            Destroy(wanderingTransform.gameObject);
        }
        
    }


    //This gets called before Update functions.
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<MeleeAIEnemy>() && Time.time > nextAttackTime)
        {
            //Attack enemy AI
            MeleeAIEnemy enemyAI = other.gameObject.GetComponent<MeleeAIEnemy>();
            nextAttackTime = Time.time + Random.Range(0.5f, 1.0f);
            enemyAI.TakeDamage(20);
        }
    }
}
