using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NavAgent : MonoBehaviour
{
    public List<Vector3> pathPoints;
    public int navMeshTriIdx;
    public int targetNavMeshTriIdx;
    public Transform target;
    public float radius;
    public string id;
    protected float speed = 12.0f;
    private List<HalfPlane> ORCAHalfPlanes;
    List<NavAgent> nearbyNavAgents = new List<NavAgent>();
    public Vector3 desiredHeading;

    protected Rigidbody rb;
    protected bool overrideNav = false;

    const float LARGE_FLOAT = 10000.0f;
    const float MIN_X_VEL = -20.0f;
    const float MAX_X_VEL = 20.0f;
    const float MIN_Y_VEL = -20.0f;
    const float MAX_Y_VEL = 20.0f;
    const float tau = 3.0f;



    // Start is called before the first frame update
    protected void Start()
    {
        desiredHeading = speed * (target.position - transform.position).normalized;
        Vector3 spriteBounds = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().bounds.size;
        radius = spriteBounds.x / 3;
        transform.forward = -Camera.main.transform.forward;
        transform.up = Camera.main.transform.up;
        pathPoints = new List<Vector3>();

        rb = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
        if (target)
        {
            Vector3 agentStartPos = new Vector3(transform.position.x, 0.0f, transform.position.z);
            Vector3 targetPosition = new Vector3(target.position.x, 0.0f, target.position.z);
            int targetTriIdx = NavigationMesh.Instance.NavMeshTriFromPos(targetPosition);
            if (targetTriIdx >= 0)
            {
                targetNavMeshTriIdx = targetTriIdx;
            }

            int agentTriIdx = NavigationMesh.Instance.NavMeshTriFromPos(agentStartPos);
            if (agentTriIdx >= 0)
            {
                navMeshTriIdx = agentTriIdx;
                pathPoints = NavigationMesh.Instance.GetShortestPath(navMeshTriIdx, targetNavMeshTriIdx, agentStartPos, targetPosition, radius);
            }
            else
            {
                //AI agent is out of bounds. Make it head towards last navigation mesh triangle
                pathPoints = new List<Vector3>() { NavigationMesh.Instance.GetTriPosition(navMeshTriIdx) };
            }

            ORCAHalfPlanes = new List<HalfPlane>();
            UpdateNearbyNavAgents();

            //Construct obstacle avoidance planes
            Player2AgentORCA(AIManager.Instance.playerAgent);
            foreach (NavAgent agentB in nearbyNavAgents)
            {
                Agent2AgentORCA(agentB);
            }

            Vector3 optimalHeading = GetOptimalHeading();
            MoveAgent(optimalHeading);
        }
        else
        {

        }
    }

    public virtual void MoveAgent(Vector3 heading)
    {

    }

    protected void UpdateNearbyNavAgents()
    {
        nearbyNavAgents = new List<NavAgent>();
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, 20.0f, (1 << 8) | (1 << 9));
        foreach (Collider enemyCollider in collidersInRange)
        {
            NavAgent navAgent = enemyCollider.gameObject.GetComponent<NavAgent>();
            if(navAgent != this)
            {
                nearbyNavAgents.Add(navAgent);
            }
        }
    }

    public Vector3 GetOptimalHeading()
    {
        bool noValidVelocity = false;
        Vector2 desiredVelocity = new Vector2(desiredHeading.x, desiredHeading.z);
        Vector2 optimalHeading = desiredVelocity;

        List<HalfPlane> bounds = new List<HalfPlane>();
        bounds.Add(new HalfPlane(Vector2.down, new Vector2(0, 10), 1.0f));
        bounds.Add(new HalfPlane(Vector2.up, new Vector2(0, -10), 1.0f));
        bounds.Add(new HalfPlane(Vector2.left, new Vector2(10, 0), 1.0f));
        bounds.Add(new HalfPlane(Vector2.right, new Vector2(-10, 0), 1.0f));

        foreach (HalfPlane halfPlane in ORCAHalfPlanes)
        {
            if (Vector2.Dot(optimalHeading - halfPlane.p, halfPlane.n) < 0)
            {
                Vector2 dir = Vector2.Perpendicular(halfPlane.n);
                LineSegment optimalInterval = new LineSegment(halfPlane.p - LARGE_FLOAT * dir, halfPlane.p + LARGE_FLOAT * dir);
                foreach (HalfPlane bound in bounds)
                {
                    float D = halfPlane.n.x * bound.n.y - halfPlane.n.y * bound.n.x;
                    float Dx = Vector2.Dot(halfPlane.n, halfPlane.p) * bound.n.y - halfPlane.n.y * Vector2.Dot(bound.n, bound.p);
                    float Dy = halfPlane.n.x * Vector2.Dot(bound.n, bound.p) - Vector2.Dot(halfPlane.n, halfPlane.p) * bound.n.x;
                    Vector2 intersection = new Vector2(Dx / D, Dy / D);

                    if (Vector2.Dot(bound.n, dir) > 0)
                    {
                        if (Vector2.Dot(intersection - optimalInterval.p1, dir) > 0)
                        {
                            optimalInterval.p1 = intersection;
                        }
                    }
                    else
                    {
                        if (Vector2.Dot(intersection - optimalInterval.p2, -dir) > 0)
                        {
                            optimalInterval.p2 = intersection;
                        }
                    }
                }

                if (Vector2.Dot(optimalInterval.Dir, dir) > 0)
                {
                    optimalHeading = Vector2.Distance(optimalInterval.p1, desiredVelocity) < Vector2.Distance(optimalInterval.p2, desiredVelocity) ? optimalInterval.p1 : optimalInterval.p2;

                    //Test perpendicular distance from desiredVelocity to optimalInterval
                    Vector2 n = Vector2.Perpendicular(optimalInterval.Dir);
                    float D = optimalInterval.Dir.x * n.y - optimalInterval.Dir.y * n.x;
                    float Dx = Vector2.Dot(optimalInterval.Dir, desiredVelocity) * n.y - optimalInterval.Dir.y * Vector2.Dot(n, optimalInterval.p1);
                    float Dy = optimalInterval.Dir.x * Vector2.Dot(n, optimalInterval.p1) - Vector2.Dot(optimalInterval.Dir, desiredVelocity) * n.x;
                    Vector2 potentialHeading = new Vector2(Dx / D, Dy / D);
                    if (Vector2.Distance(potentialHeading, desiredVelocity) < Vector2.Distance(optimalHeading, desiredVelocity) &&
                        Vector2.Dot(optimalInterval.p2 - potentialHeading, optimalInterval.p1 - potentialHeading) < 0)
                    {
                        optimalHeading = potentialHeading;
                    }
                }
                else
                {
                    noValidVelocity = true;
                    break;
                }
            }
            bounds.Add(halfPlane);
        }



        if (noValidVelocity)
        {
            //There is no velocity that avoids obstacles. Find velocity that satisfies the weighted least squares
            //distances from each half plane

            float[,] AT_A = new float[2, 2] { {0.0f, 0.0f },
                                              {0.0f, 0.0f } };
            float[] AT_b = new float[2] { 0.0f, 0.0f };

            foreach (HalfPlane halfPlane in ORCAHalfPlanes)
            {
                Vector2 Ai = halfPlane.n / halfPlane.weight;
                float bi = Vector2.Dot(halfPlane.n, halfPlane.p) / halfPlane.weight;
                AT_A[0, 0] += Ai[0] * Ai[0];
                AT_A[1, 0] += Ai[1] * Ai[0];
                AT_A[0, 1] += Ai[0] * Ai[1];
                AT_A[1, 1] += Ai[1] * Ai[1];
                AT_b[0] += Ai[0] * bi;
                AT_b[1] += Ai[1] * bi;
            }

            float[,] AT_A_inv = new float[2, 2];
            float det = AT_A[0, 0] * AT_A[1, 1] - AT_A[1, 0] * AT_A[0, 1];
            AT_A_inv[0, 0] = AT_A[1, 1] / det;
            AT_A_inv[1, 0] = -AT_A[1, 0] / det;
            AT_A_inv[0, 1] = -AT_A[0, 1] / det;
            AT_A_inv[1, 1] = AT_A[0, 0] / det;

            float xVel = AT_A_inv[0, 0] * AT_b[0] + AT_A_inv[0, 1] * AT_b[1];
            float zVel = AT_A_inv[1, 0] * AT_b[0] + AT_A_inv[1, 1] * AT_b[1];
            optimalHeading = new Vector2(xVel, zVel);
            optimalHeading = Mathf.Clamp(optimalHeading.magnitude, 0.0f, 10.0f) * optimalHeading.normalized;
        }
        return new Vector3(optimalHeading.x, 0.0f, optimalHeading.y);
    }

    //Create Optimal reciprocal Collision Avoidance half plane
    void Agent2AgentORCA(NavAgent agentB)
    {
        Vector3 dp = agentB.transform.position - transform.position;
        Vector2 vCenter = new Vector2(dp.x, dp.z);
        Vector2 vCenterScaled = new Vector2(dp.x, dp.z) / tau;
        float r = radius + agentB.radius;
        float r_scaled = r / tau;
        float phi = Mathf.Abs(Mathf.Asin(Mathf.Min(r / vCenter.magnitude, 1.0f))) * Mathf.Rad2Deg;
        float alpha = 90.0f - phi;

        Vector2 velObstacleDir = (vCenter - vCenterScaled).normalized;

        Vector2 vOptA = new Vector2(desiredHeading.x, desiredHeading.z);
        Vector2 vOptB = new Vector2(agentB.desiredHeading.x, agentB.desiredHeading.z);
        Vector2 vx = vOptA - vOptB;
        Vector2 vx_c = vx - vCenterScaled;

        float theta = Vector2.SignedAngle(velObstacleDir, vx_c);
        //float theta = Vector3.SignedAngle(new Vector3(velObstacleDir[0], 0, velObstacleDir[1]), new Vector3(vx_c[0], 0, vx_c[1]), Vector3.up);
        Vector2 u;
        Vector2 n;

        if (theta < 0.0f && theta > -(180.0f - alpha))
        {
            Vector2 b1 = Quaternion.AngleAxis(-phi, new Vector3(0, 0, 1)) * vCenter;
            u = Vector2.Dot(vx, b1.normalized) * b1.normalized - vx;
            n = -Vector2.Perpendicular(b1).normalized;
        }
        else if (theta >= 0.0f && theta < (180.0f - alpha))
        {
            Vector2 b2 = Quaternion.AngleAxis(phi, new Vector3(0, 0, 1)) * vCenter;
            u = Vector2.Dot(vx, b2.normalized) * b2.normalized - vx;
            n = Vector2.Perpendicular(b2).normalized;
        }
        else
        {
            u = (r_scaled * vx_c.normalized + vCenterScaled) - vx;
            n = vx_c.magnitude < Mathf.Epsilon ? -vCenter.normalized : vx_c.normalized;
        }

        ORCAHalfPlanes.Add(new HalfPlane(n, vOptA + 0.5f * u, (vCenter - vCenterScaled).magnitude));
    }

    void Player2AgentORCA(PlayerMovementController player)
    {
        Vector3 dp = transform.position - player.transform.position;
        Vector2 vCenter = new Vector2(dp.x, dp.z);
        Vector2 vCenterScaled = new Vector2(dp.x, dp.z) / tau;
        float r = player.radius + radius;
        float r_scaled = r / tau;
        float phi = Mathf.Abs(Mathf.Asin(Mathf.Min(r / vCenter.magnitude, 1.0f))) * Mathf.Rad2Deg;
        float alpha = 90.0f - phi;

        Vector2 velObstacleDir = (vCenter - vCenterScaled).normalized;

        Vector2 vOptA = new Vector2(player.velocity.x, player.velocity.z);
        Vector2 vOptB = new Vector2(desiredHeading.x, desiredHeading.z);
        Vector2 vx = vOptA - vOptB;
        Vector2 vx_c = vx - vCenterScaled;

        float theta = Vector2.SignedAngle(velObstacleDir, vx_c);
        //float theta = Vector3.SignedAngle(new Vector3(velObstacleDir[0], 0, velObstacleDir[1]), new Vector3(vx_c[0], 0, vx_c[1]), Vector3.up);
        Vector2 u;
        Vector2 n;

        if (theta < 0.0f && theta > -(180.0f - alpha))
        {
            Vector2 b1 = Quaternion.AngleAxis(-phi, new Vector3(0, 0, 1)) * vCenter;
            u = Vector2.Dot(vx, b1.normalized) * b1.normalized - vx;
            n = -Vector2.Perpendicular(b1).normalized;
        }
        else if (theta >= 0.0f && theta < (180.0f - alpha))
        {
            Vector2 b2 = Quaternion.AngleAxis(phi, new Vector3(0, 0, 1)) * vCenter;
            u = Vector2.Dot(vx, b2.normalized) * b2.normalized - vx;
            n = Vector2.Perpendicular(b2).normalized;
        }
        else
        {
            u = (r_scaled * vx_c.normalized + vCenterScaled) - vx;
            n = vx_c.magnitude < Mathf.Epsilon ? -vCenter.normalized : vx_c.normalized;
        }

        ORCAHalfPlanes.Add(new HalfPlane(-n, vOptB - u, 0.01f * (vCenter - vCenterScaled).magnitude));
    }

    protected IEnumerator PauseNav(float t)
    {
        overrideNav = true;
        yield return new WaitForSeconds(t);
        overrideNav = false;
    }
}

