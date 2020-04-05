using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : NavAgent
{
    HealthBar healthBar;
    Animator animator;
    BehaviorTree bt;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        animator = GetComponentInChildren<Animator>();

        Vector3 spriteBounds = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().bounds.size;
        radius = spriteBounds.x / 3;
        transform.forward = -Camera.main.transform.forward;
        transform.up = Camera.main.transform.up;

        maxSpeed = 12.0f;
        healthBar = transform.Find("HealthBarCanvas").gameObject.GetComponent<HealthBar>();

        bt = new BehaviorTree(CreateBehaviourTree());
        bt.blackboard["foo"] = false;
        //Debug.Log("no errors yet");
        //Debug.Log(bt.blackboard.Get<bool>("foo"));
    }

    private void FixedUpdate()
    {
        Vector3 targetLoc = pathPoints[0];
        Vector3 faceDir = (targetLoc - transform.position).normalized;

        animator.SetFloat("movement", rb.velocity.magnitude);
        animator.SetFloat("facingY", faceDir.z);
        animator.SetFloat("facingX", faceDir.x);

        if(rb.velocity.magnitude > 0.01f)
        {
            animator.SetFloat("dy", rb.velocity.z);
            animator.SetFloat("dx", rb.velocity.x);
        }

        //MoveAgent();
        bt.Execute();
    }

    public override void MoveAgent()
    {
        if (!overrideNav)
        {
            Vector3 curPos = new Vector3(transform.position.x, 0.0f, transform.position.z);
            //rb.MovePosition(curPos + optimalVelocity * Time.fixedDeltaTime);
            rb.velocity = optimalVelocity;

            //Smooth movement
            if (pathPoints.Count == 1)
            {
                desiredHeading = optimalVelocity;
                Vector3.SmoothDamp(transform.position, pathPoints[0], ref desiredHeading, 0.3f, maxSpeed);
            }
            else
            {
                desiredHeading = Vector3.Lerp(optimalVelocity, maxSpeed * (pathPoints[0] - curPos).normalized, 5.0f * Time.fixedDeltaTime);
                //desiredHeading = optimalVelocity;
            }
            Debug.DrawLine(curPos, curPos + desiredHeading, Color.cyan);
        }
        else
        {
            desiredHeading = rb.velocity;
        }

    }

    
    Root CreateBehaviourTree()
    {
     
        /*
        Sequence engagePlayer = new Sequence("engagePlayer",
            new BlackboardCondition(, "player_in_sight", Operator.IS_EQUAL, true),
            chooseExposedPlayer,
            new FaceTarget(),
            new Succeeder(new BlackboardCondition(new Attack(), "in_attack_range", Operator.IS_SET)),
            new MoveAI()
            );

        Sequence engageBarricade = new Sequence("engageBarricade",
            new BlackboardCondition(, "barricade_in_sight", Operator.IS_EQUAL, true)
            new FaceTarget(),
            new MoveAI()
            );

        Selector main = new Selector("main",
            //Use event here
            new BlackboardCondition(, "is_knocked_back", Operator.IS_EQUAL, true)
            engagePlayer,
            engageBarricade,
            new Wander()
            );

        Repeater repeater = new Repeater(main);
        return repeater;
        */

        
        Root root = new Root("root",
            new Service("Service: 0", GetComponent<MonoBehaviour>(), 1.0f, () => {
                bt.blackboard["foo"] = !bt.blackboard.Get<bool>("foo");
                bt.NotifyListeningNodesForEvent("foo");
            },
                new Selector("Selector: 0",
                    new BlackboardCondition("Blackboard Condition: 0", "foo", Operator.IS_EQUAL, true,
                        new Sequence("Sequence: 0",
                            new BTAction("Action: Foo", () =>
                            {
                                Debug.Log("foo");
                                return TaskResult.SUCCESS;
                            }
                            ),
                            new Wait("Wait: 0")
                        )
                    ),

                    new Sequence("Sequence: 1",
                        new BTAction("Action: Bar", () =>
                        {
                            Debug.Log("bar");
                            return TaskResult.SUCCESS;
                        }
                        ),
                        new Wait("Wait: 1")
                    )
                )
            )
            
        );

        return root;
    }

    private void OnTriggerEnter(Collider other)
    {
        //bt.SetBlackboardKey("in_attack_range", true);
    }

}
