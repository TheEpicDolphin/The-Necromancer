using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VecUtils;

public class NavAgent : MonoBehaviour
{
    public List<Vector3> pathPoints;
    public int navMeshTriIdx;
    public int targetNavMeshTriIdx;
    public Transform target;
    public float radius;
    public string id;
    protected float speed = 5.0f;
    private List<HalfPlane2D> ORCAHalfPlanes;
    List<NavAgent> nearbyNavAgents = new List<NavAgent>();
    public Vector3 desiredHeading;
    protected Vector3 tempPreferredHeading;

    protected Rigidbody rb;
    protected bool overrideNav = false;
    int avoidanceLayer = 3;

    const float LARGE_FLOAT = 10000.0f;
    const float tau = 1.5f;
    float nearbyObstacleRadius; 


    // Start is called before the first frame update
    protected void Start()
    {
        tempPreferredHeading = speed * (target.position - transform.position).normalized;
        desiredHeading = tempPreferredHeading;
        pathPoints = new List<Vector3>();
        rb = GetComponent<Rigidbody>();
        nearbyObstacleRadius = 2 * speed * tau;

    }

    private void Update()
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

            ORCAHalfPlanes = new List<HalfPlane2D>();
            UpdateNearbyNavAgents();

            //Construct obstacle avoidance planes
            //Player2AgentORCA(AIManager.Instance.playerAgent);
            foreach (NavAgent agentB in nearbyNavAgents)
            {
                AgentORCA(agentB);
            }

            Vector3 optimalHeading = GetOptimalHeading();
            MoveAgent(optimalHeading);
        }
        else
        {

        }
    }

    private void LateUpdate()
    {
        desiredHeading = tempPreferredHeading;
    }

    public virtual void MoveAgent(Vector3 heading)
    {

    }

    protected void UpdateNearbyNavAgents()
    {
        nearbyNavAgents = new List<NavAgent>();
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, nearbyObstacleRadius, (1 << 12));
        foreach (Collider enemyCollider in collidersInRange)
        {   
            NavAgent navAgent = enemyCollider.GetComponentInParent<NavAgent>();
            if(navAgent != this)
            {
                nearbyNavAgents.Add(navAgent);
            }
        }
    }

    public Vector3 GetOptimalHeading()
    {
        Vector2 desiredVelocity = new Vector2(desiredHeading.x, desiredHeading.z);
        Vector2 optimalHeading = desiredVelocity;

        if (!LinearProgram2D(desiredVelocity, ORCAHalfPlanes, false, ref optimalHeading))
        {

            LinearProgramDenselyPacked(ORCAHalfPlanes, ref optimalHeading);

            /*
            //3d Linear programming approach to minimize maximum error
            List<HalfPlane> halfPlanes = new List<HalfPlane>();
            foreach (HalfPlane2D halfPlane2d in ORCAHalfPlanes)
            {
                Vector3 n = new Vector3(halfPlane2d.n.x, halfPlane2d.n.y, 1).normalized;
                Vector3 p = new Vector3(halfPlane2d.p.x, halfPlane2d.p.y, 0);
                halfPlanes.Add(new HalfPlane(n, p));
            }
            Vector3 temp = LinearProgram3D(LARGE_FLOAT * new Vector3(0, 0, -1), halfPlanes);
            optimalHeading = new Vector2(temp.x, temp.y);
            */

            /*
            //There is no velocity that avoids obstacles. Find velocity that satisfies the weighted least squares
            //distances from each half plane
            float[,] AT_A = new float[2, 2] { {0.0f, 0.0f },
                                              {0.0f, 0.0f } };
            float[] AT_b = new float[2] { 0.0f, 0.0f };

            foreach (HalfPlane2D halfPlane in ORCAHalfPlanes)
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
            */
        }

        return Mathf.Clamp(optimalHeading.magnitude, 0.0f, speed) * new Vector3(optimalHeading.x, 0.0f, optimalHeading.y).normalized;
    }

    /*
     * Returns true if there is valid solution to 2d linear program. False otherwise.
     */
    private bool LinearProgram2D(Vector2 c, List<HalfPlane2D> halfPlanes, bool optDirection, ref Vector2 optimalHeading)
    {
        optimalHeading = c;
        List<HalfPlane2D> bounds = new List<HalfPlane2D>();

        foreach (HalfPlane2D halfPlane in halfPlanes)
        {
            if (Vector2.Dot(optimalHeading - halfPlane.p, halfPlane.n) < 0)
            {
                Vector2 dir = Vector2.Perpendicular(halfPlane.n);
                LineSegment optimalInterval = new LineSegment(halfPlane.p - LARGE_FLOAT * dir, halfPlane.p + LARGE_FLOAT * dir);
                foreach (HalfPlane2D bound in bounds)
                {
                    Vector2 intersection = Vector2.zero;
                    HalfPlane2D.Intersection(halfPlane, bound, ref intersection);

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
                    if (optDirection)
                    {
                        
                        optimalHeading = Vector2.Dot(optimalInterval.p1, c) > Vector2.Dot(optimalInterval.p2, c) ? optimalInterval.p1 : optimalInterval.p2;
                    }
                    else
                    {
                        optimalHeading = Vector2.Distance(optimalInterval.p1, c) < Vector2.Distance(optimalInterval.p2, c) ? optimalInterval.p1 : optimalInterval.p2;

                        //Test perpendicular distance from desiredVelocity to optimalInterval
                        Vector2 n = Vector2.Perpendicular(optimalInterval.Dir);
                        float D = optimalInterval.Dir.x * n.y - optimalInterval.Dir.y * n.x;
                        float Dx = Vector2.Dot(optimalInterval.Dir, c) * n.y - optimalInterval.Dir.y * Vector2.Dot(n, optimalInterval.p1);
                        float Dy = optimalInterval.Dir.x * Vector2.Dot(n, optimalInterval.p1) - Vector2.Dot(optimalInterval.Dir, c) * n.x;
                        Vector2 potentialHeading = new Vector2(Dx / D, Dy / D);
                        if (Vector2.Distance(potentialHeading, c) < Vector2.Distance(optimalHeading, c) &&
                            Vector2.Dot(optimalInterval.p2 - potentialHeading, optimalInterval.p1 - potentialHeading) < 0)
                        {
                            optimalHeading = potentialHeading;
                        }
                    }
                }
                else
                {
                    //No solution
                    return false;
                }
            }
            bounds.Add(halfPlane);
        }
        return true;
    }

    private void LinearProgramDenselyPacked(List<HalfPlane2D> halfPlanes, ref Vector2 optimalHeading)
    {
        for(int i = 0; i < halfPlanes.Count; i++)
        {
            if (Vector3.Dot(optimalHeading - halfPlanes[i].p, halfPlanes[i].n) < 0)
            {
                List<HalfPlane2D> equidistantHalfPlanes = new List<HalfPlane2D>();
                Vector2 intersectionPoint = Vector2.zero;
                for (int j = 0; j < i; j++)
                {
                    if (!HalfPlane2D.Intersection(halfPlanes[i], halfPlanes[j], ref intersectionPoint))
                    {
                        /* halfplane i and j are parallel. */
                        if (Vector2.Dot(halfPlanes[i].n, halfPlanes[j].n) > 0.0f)
                        {
                            /* halfplane i and j face in the same direction. */
                            continue;
                        }
                        else
                        {
                            /* halfplane i and halfplane j face in opposite directions. */
                            intersectionPoint = 0.5f * (halfPlanes[i].p + halfPlanes[j].p);
                        }
                    }
                    equidistantHalfPlanes.Add(new HalfPlane2D((halfPlanes[j].n - halfPlanes[i].n).normalized, intersectionPoint));
                }

                Vector2 tempOptimalHeading = optimalHeading;
                if (!LinearProgram2D(halfPlanes[i].n, equidistantHalfPlanes, true, ref optimalHeading))
                {
                    //This should in principle not happen. But due to floating point errors, it may.
                    optimalHeading = tempOptimalHeading;
                }

                /*
                foreach (HalfPlane2D hp in equidistantHalfPlanes)
                {
                    Debug.Log(Vector3.Dot(hp.n, optimalHeading - hp.p));
                }
                */
            }
        }
    }

    
    private Vector3 LinearProgram3D(Vector3 c, List<HalfPlane> halfPlanes)
    {
        Vector3 optimalHeading = c;
        List<HalfPlane> bounds = new List<HalfPlane>();

        foreach (HalfPlane halfPlane in halfPlanes)
        {
            if (Vector3.Dot(optimalHeading - halfPlane.p, halfPlane.n) < 0)
            {
                //Vector3 i = Vector3.right;
                //Vector3 j = Vector3.up;
                //Vector3.OrthoNormalize(ref halfPlane.n, ref i, ref j);

                Vector3 j = Vector3.ProjectOnPlane(Vector3.up, halfPlane.n).normalized;
                if(j.magnitude <= Mathf.Epsilon)
                {
                    j = Vector3.ProjectOnPlane(Vector3.right, halfPlane.n).normalized;
                }
                Vector3 i = Vector3.Cross(j, halfPlane.n);
                

                // Copy the three new basis vectors into the rows of a matrix
                // (since it is actually a 4x4 matrix, the bottom right corner
                // should also be set to 1).
                Matrix4x4 toHalfPlaneSpace = Matrix4x4.identity;
                toHalfPlaneSpace.SetRow(0, i);
                toHalfPlaneSpace.SetRow(1, j);
                toHalfPlaneSpace.SetRow(2, halfPlane.n);
                
                Matrix4x4 translate = Matrix4x4.identity;
                translate.SetColumn(3, -halfPlane.p);
                translate[3, 3] = 1.0f;
                toHalfPlaneSpace = toHalfPlaneSpace * translate;

                //Transpose is same as inverse for orthogonal matrix
                Matrix4x4 fromHalfPlaneSpace = toHalfPlaneSpace.inverse;

                Vector3 temp = toHalfPlaneSpace.MultiplyPoint(c);
                Vector2 cTransformed = new Vector2(temp.x, temp.y);
                List<HalfPlane2D> halfPlanesTransformed = new List<HalfPlane2D>();

                foreach (HalfPlane bound in bounds)
                {
                    Vector3 lineDir = Vector3.zero;
                    Vector3 linePos = Vector3.zero;
                    
                    if(HalfPlane.Intersection(halfPlane, bound, ref lineDir, ref linePos))
                    {
                        temp = toHalfPlaneSpace.MultiplyVector(lineDir);
                        Vector2 lineDirTransformed = new Vector2(temp.x, temp.y);
                        temp = toHalfPlaneSpace.MultiplyPoint(linePos);
                        Vector2 linePosTransformed = new Vector2(temp.x, temp.y);
                        //Debug.Log(temp);
                        halfPlanesTransformed.Add(new HalfPlane2D(-Vector2.Perpendicular(lineDirTransformed).normalized, linePosTransformed));
                    }
                }

                Vector2 vStar = Vector2.zero;
                Vector2 tempOptimalHeading = optimalHeading;
                if (LinearProgram2D(cTransformed, halfPlanesTransformed, false, ref vStar))
                {
                    optimalHeading = fromHalfPlaneSpace.MultiplyPoint(vStar);
                    //Vector3 potentialHeading = fromHalfPlaneSpace.MultiplyPoint(vStar);
                    //optimalHeading = Vector3.Distance(potentialHeading, c) < Vector3.Distance(optimalHeading, c) ? potentialHeading : optimalHeading;
                }
                else
                {
                    //This should in principle not happen
                    optimalHeading = tempOptimalHeading;
                }
            }
            bounds.Add(halfPlane);
        }

        /*
        foreach(HalfPlane halfPlane in halfPlanes)
        {
            Debug.Log(Vector3.Dot(halfPlane.n, optimalHeading - halfPlane.p));
        }
        */

        return optimalHeading;
    }
    


    //Create Optimal reciprocal Collision Avoidance half plane
    void AgentORCA(NavAgent agentB)
    {
        Vector3 dp = agentB.transform.position - transform.position;
        Vector2 vCenter = new Vector2(dp.x, dp.z);
        Vector2 vCenterScaled = new Vector2(dp.x, dp.z) / tau;
        float r = radius + agentB.radius;
        float r_scaled = r / tau;

        Vector2 velObstacleDir = (vCenter - vCenterScaled).normalized;

        Vector2 vOptA = new Vector2(desiredHeading.x, desiredHeading.z);
        Vector2 vOptB = new Vector2(agentB.desiredHeading.x, agentB.desiredHeading.z);
        Vector2 vx = vOptA - vOptB;
        Vector2 vx_c = vx - vCenterScaled;

        Vector2 u;
        Vector2 n;

        float phi = Mathf.Abs(Mathf.Asin(Mathf.Min(r / vCenter.magnitude, 1.0f))) * Mathf.Rad2Deg;
        float alpha = 90.0f - phi;

        float theta = Vector2.SignedAngle(velObstacleDir, vx_c);
        //float theta = Vector3.SignedAngle(new Vector3(velObstacleDir[0], 0, velObstacleDir[1]), new Vector3(vx_c[0], 0, vx_c[1]), Vector3.up);

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

        ORCAHalfPlanes.Add(new HalfPlane2D(n, vOptA + 0.5f * u));

        //float d = Mathf.Min(Vector3.Distance(agentB.target.position, agentB.transform.position), 20.0f);
        //float f = d / 20.0f;
        //ORCAHalfPlanes.Add(new HalfPlane2D(n, vOptA + 0.5f * u, Mathf.Pow(f, 10)));

        /*
        if(vCenter.magnitude < 1.5f * r)
        {
            ORCAHalfPlanes.Add(new HalfPlane2D(n, vOptA + 0.5f * u, 0.01f * vCenter.magnitude));
        }
        else
        {
            ORCAHalfPlanes.Add(new HalfPlane2D(n, vOptA + 0.5f * u, vCenter.magnitude));
        }
        */


        /*
        if (avoidanceLayer == agentB.avoidanceLayer)
        {
            ORCAHalfPlanes.Add(new HalfPlane2D(n, vOptA + 0.5f * u, vCenter.magnitude));
        }
        else if (avoidanceLayer < agentB.avoidanceLayer)
        {
            //ORCAHalfPlanes.Add(new HalfPlane2D(n, vOptA + 0.0f * u, vCenter.magnitude));
        }
        else if (avoidanceLayer > agentB.avoidanceLayer)
        {
            ORCAHalfPlanes.Add(new HalfPlane2D(n, vOptA + 1.0f * u, 0.01f * vCenter.magnitude));
        }
        */

    }



    void PlayerORCA(PlayerMovementController player)
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

        ORCAHalfPlanes.Add(new HalfPlane2D(-n, vOptB - u, 0.01f * (vCenter - vCenterScaled).magnitude));
    }

    protected IEnumerator PauseNav(float t)
    {
        overrideNav = true;
        yield return new WaitForSeconds(t);
        overrideNav = false;
    }
}

