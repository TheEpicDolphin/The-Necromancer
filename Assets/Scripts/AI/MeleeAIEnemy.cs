using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeleeAIEnemy : NavAgent
{
    public enum AIState
    {
        Attacking,
        Dying,
        Dead,
        Despawning
    }
    AIState state;
    public Transform playerTrans;
    float ATTACK_RADIUS = 30.0f;
    float HIT_STRENGTH = 20.0f;
    public UnityEvent deathEvent = new UnityEvent();
    List<MeleeAIAlly> nearbyAllies = new List<MeleeAIAlly>();
    Coroutine nearbyAlliesCoroutine;

    //How much more the enemies prefer attacking the player over allies
    static float PLAYER_PREFERENCE = 1.5f;

    HealthBar healthBar;
    float nextAttackTime;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        nextAttackTime = Time.time;
        speed = 12.0f;
        state = AIState.Attacking;
        healthBar = transform.Find("HealthBarCanvas").gameObject.GetComponent<HealthBar>();
        nearbyAlliesCoroutine = StartCoroutine(UpdateNearbyAllies());

    }

    void Update()
    {
        //Attack player
        switch (state)
        {
            case AIState.Attacking:
                //Navigating to fight player
                break;
            case AIState.Dying:
                //Play dying animation with coroutine maybe
                state = AIState.Dead;
                break;
            case AIState.Dead:
                //In this state, the enemy can potentially be revived by the player. If we wait too long, the enemy
                //will despawn
                break;
            case AIState.Despawning:
                break;
        }
    }

    public bool IsDead()
    {
        return state == AIState.Dead;
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
            if(pathPoints.Count == 1)
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

    private void OnDestroy()
    {
        
    }

    IEnumerator UpdateNearbyAllies()
    {
        while (true)
        {
            nearbyAllies = new List<MeleeAIAlly>();
            Collider[] collidersInRange = Physics.OverlapSphere(transform.position, 5.0f, 1 << 8);
            foreach (Collider enemyCollider in collidersInRange)
            {
                MeleeAIAlly allyAI = enemyCollider.gameObject.GetComponent<MeleeAIAlly>();
                nearbyAllies.Add(allyAI);
            }


            Transform closestAgent = playerTrans;
            float minDistance = Vector3.Distance(closestAgent.position, transform.position);
            for (int i = 0; i < nearbyAllies.Count; i++)
            {
                float dist = Vector3.Distance(nearbyAllies[i].transform.position, transform.position);
                if (dist < minDistance)
                {
                    closestAgent = nearbyAllies[i].transform;
                    minDistance = dist;
                }
            }
            target = closestAgent;

            yield return new WaitForSeconds(0.1f);
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
            StopCoroutine(nearbyAlliesCoroutine);
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

    //This gets called before Update functions.
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<MeleeAIAlly>() && Time.time > nextAttackTime)
        {
            //Attack ally AI
            MeleeAIAlly allyAI = other.gameObject.GetComponent<MeleeAIAlly>();
            nextAttackTime = Time.time + Random.Range(0.5f, 1.0f);
            //nextAttackTime = Time.time + 0.5f;
            Vector3 knockbackDir = (allyAI.transform.position - this.transform.position).normalized;
            allyAI.TakeDamage(1, HIT_STRENGTH * knockbackDir);
        }
    }
}
