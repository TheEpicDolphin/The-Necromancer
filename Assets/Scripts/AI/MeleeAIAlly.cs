using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeleeAIAlly : NavAgent
{
    public enum AIState
    {
        AttackingEnemy,
        ReturningToMaster,
        Patrolling,
        Dying,
        Despawning
    }
    AIState state;
    public Transform master;
    Transform wanderingTransform;

    float nextWanderTime;
    float WANDER_RADIUS = 30.0f;
    float HIT_STRENGTH = 20.0f;

    public UnityEvent deathEvent = new UnityEvent();
    List<MeleeAIEnemy> nearbyEnemies = new List<MeleeAIEnemy>();
    Coroutine nearbyEnemiesCoroutine;
    float nextAttackTime;

    HealthBar healthBar;

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

        healthBar = transform.Find("HealthBarCanvas").gameObject.GetComponent<HealthBar>();
        nearbyEnemiesCoroutine = StartCoroutine(UpdateNearbyEnemies());
    }

    void Update()
    {
        
        switch (state)
        {
            case AIState.AttackingEnemy:
                if ((master.position - transform.position).magnitude >= WANDER_RADIUS)
                {
                    state = AIState.ReturningToMaster;
                    target = master;
                }
                break;
            case AIState.ReturningToMaster:
                if ((master.position - transform.position).magnitude < WANDER_RADIUS)
                {
                    state = AIState.Patrolling;
                    target = wanderingTransform;
                }
                break;
            case AIState.Patrolling:
                if ((master.position - transform.position).magnitude >= WANDER_RADIUS)
                {
                    state = AIState.ReturningToMaster;
                    target = master;
                }
                else
                {
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
                Destroy(this.gameObject);
                break;
            case AIState.Despawning:
                break;
        }
    }

    public override void MoveAgent(Vector3 heading)
    {
        if (!overrideNav)
        {
            rb.velocity = heading;
            //transform.Translate(heading * Time.deltaTime, Space.World);
            Vector3 curPos = new Vector3(transform.position.x, 0.1f, transform.position.z);
            //desiredHeading = TARGET_SPEED * (pathPoints[0] - curPos).normalized;
            //Smooth movement
            if (pathPoints.Count == 1)
            {
                desiredHeading = heading;
                Vector3.SmoothDamp(transform.position, pathPoints[0], ref desiredHeading, 0.3f, speed);
            }
            else
            {
                desiredHeading = Vector3.Lerp(heading, speed * (pathPoints[0] - curPos).normalized, 5.0f * Time.deltaTime);
            }
            Debug.DrawLine(curPos, curPos + desiredHeading, Color.cyan);
        }
        else
        {
            desiredHeading = rb.velocity;
        }
    }

    public void TakeDamage(int damage, Vector3 knockbackDir)
    {
        int currentHealth = healthBar.GetCurrentHealth();
        
        if (healthBar.DecrementHealth(damage) && currentHealth > 0)
        {
            state = AIState.Dying;
            //Disable collider to avoid future triggers
            gameObject.GetComponent<Collider>().enabled = false;
            deathEvent.Invoke();
            StopCoroutine(nearbyEnemiesCoroutine);
            target = null;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.velocity = Vector3.zero;
        }
        else
        {
            //Add knockback to AI
            rb.AddForce(damage * knockbackDir, ForceMode.Impulse);
            StartCoroutine(PauseNav(0.1f));
        }
    }

    private void OnDestroy()
    {
        if (wanderingTransform)
        {
            Destroy(wanderingTransform.gameObject);
        }
        
    }

    IEnumerator UpdateNearbyEnemies()
    {
        while (true)
        {
            nearbyEnemies = new List<MeleeAIEnemy>();
            Collider[] collidersInRange = Physics.OverlapSphere(transform.position, 15.0f, 1 << 9);
            foreach (Collider enemyCollider in collidersInRange)
            {
                MeleeAIEnemy enemyAI = enemyCollider.gameObject.GetComponent<MeleeAIEnemy>();
                if (!enemyAI.IsDead())
                {
                    nearbyEnemies.Add(enemyAI);
                }
            }


            if (nearbyEnemies.Count == 0)
            {
                state = AIState.Patrolling;
                target = wanderingTransform;
            }
            else
            {
                //TODO: make enemies that are already surrounded by allies less desirable
                Transform closestAgent = nearbyEnemies[0].transform;
                float minDistance = Vector3.Distance(closestAgent.position, transform.position);
                for (int i = 1; i < nearbyEnemies.Count; i++)
                {
                    float dist = Vector3.Distance(nearbyEnemies[i].transform.position, transform.position);
                    if (dist < minDistance)
                    {
                        closestAgent = nearbyEnemies[i].transform;
                        minDistance = dist;
                    }
                }
                state = AIState.AttackingEnemy;
                target = closestAgent;
            }
            yield return new WaitForSeconds(0.1f);
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
            //nextAttackTime = Time.time + 0.5f;

            Vector3 knockbackDir = (enemyAI.transform.position - this.transform.position).normalized;
            enemyAI.TakeDamage(1, HIT_STRENGTH * knockbackDir);
        }
    }
}
