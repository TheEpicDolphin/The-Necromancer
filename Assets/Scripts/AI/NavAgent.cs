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
    protected float maxSpeed = 5.0f;
    private List<HalfPlane2D> ORCAHalfPlanes;
    List<NavAgent> nearbyNavAgents = new List<NavAgent>();
    public Vector3 desiredHeading = Vector3.zero;
    protected Vector3 optimalVelocity = Vector3.zero;

    protected Rigidbody rb;
    protected bool overrideNav = false;
    int avoidanceLayer = 3;

    const float LARGE_FLOAT = 100000.0f;
    const float tau = 1.5f;
    float nearbyObstacleRadius;

    Coroutine updateNavCoroutine;


    // Start is called before the first frame update
    protected void Start()
    {
        pathPoints = new List<Vector3>();
        rb = GetComponent<Rigidbody>();
        nearbyObstacleRadius = 2 * maxSpeed * tau;
        updateNavCoroutine = StartCoroutine(UpdateNav());
    }



    IEnumerator UpdateNav()
    {
        while (true)
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
            }
            else
            {
                //TODO: what if there is no target
            }

            yield return new WaitForSeconds(0.15f);
        }
    }

    private void LateUpdate()
    {
        ORCAHalfPlanes = new List<HalfPlane2D>();
        UpdateNearbyNavAgents();

        //Construct obstacle avoidance planes
        //Player2AgentORCA(AIManager.Instance.playerAgent);
        foreach (NavAgent agentB in nearbyNavAgents)
        {
            AgentORCA(agentB);
        }

        optimalVelocity = GetOptimalHeading();

        /*
        float d = Vector3.Distance(transform.position, target.position);
        if (d < radius)
        {
            avoidanceLayer = 0;
        }
        else if (d < 3 * radius)
        {
            avoidanceLayer = 1;
        }
        else
        {
            avoidanceLayer = 2;
        }
        */
    }

    public virtual void MoveAgent()
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

        if (!LinearProgram2D(desiredVelocity, ORCAHalfPlanes, false, maxSpeed, ref optimalHeading))
        {
            optimalHeading = desiredVelocity;
            //optimalHeading = Vector2.zero;
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
            optimalHeading = Mathf.Clamp(optimalHeading.magnitude, 0.0f, maxSpeed) * optimalHeading.normalized;
            */
        }

        return new Vector3(optimalHeading.x, 0.0f, optimalHeading.y);
    }

    private bool LinearProgram1D(Vector2 c, HalfPlane2D halfplane, List<HalfPlane2D> bounds, bool optDirection, float velRadius, ref Vector2 optimalHeading)
    {
        Vector2 dir = -Vector2.Perpendicular(halfplane.n);
        float t0 = Vector2.Dot(dir, halfplane.p);
        float d0 = Vector2.Dot(halfplane.n, halfplane.p);
        float Tmag = velRadius * velRadius - d0 * d0;

        if(Tmag < 0.0f)
        {
            //No velocity on the line satisfies the radius constraint
            return false;
        }

        float T = Mathf.Sqrt(Tmag);
        float tLeft = -t0 - T;
        float tRight = -t0 + T;

        foreach(HalfPlane2D bound in bounds)
        {
            Vector2 boundDir = -Vector2.Perpendicular(bound.n);
            float denominator = Vector2.Dot(halfplane.n, boundDir);
            float numerator = Vector2.Dot(bound.n, halfplane.p - bound.p);

            if (Mathf.Abs(denominator) <= Mathf.Epsilon)
            {
                /* Lines lineNo and i are (almost) parallel. */
                if (numerator < 0.0f)
                {
                    return false;
                }

                continue;
            }
            
            float t = numerator / denominator;

            if (denominator >= 0.0f)
            {
                /* Line i bounds line lineNo on the right. */
                tRight = Mathf.Min(tRight, t);
            }
            else
            {
                /* Line i bounds line lineNo on the left. */
                tLeft = Mathf.Max(tLeft, t);
            }

            if (tLeft > tRight)
            {
                return false;
            }
        }

        if (optDirection)
        {
            /* Optimize direction. */
            if (Vector2.Dot(c, dir) > 0.0f)
            {
                /* Take right extreme. */
                optimalHeading = halfplane.p + tRight * dir;
            }
            else
            {
                /* Take left extreme. */
                optimalHeading = halfplane.p + tLeft * dir;
            }
        }
        else
        {
            /* Optimize closest point. */
            float t = Vector2.Dot(dir, (c - halfplane.p));

            if (t < tLeft)
            {
                optimalHeading = halfplane.p + tLeft * dir;
            }
            else if (t > tRight)
            {
                optimalHeading = halfplane.p + tRight * dir;
            }
            else
            {
                optimalHeading = halfplane.p + t * dir;
            }
        }

        return true;

    }

    /*
     * Returns true if there is valid solution to 2d linear program. False otherwise.
     */
    private bool LinearProgram2D(Vector2 c, List<HalfPlane2D> halfPlanes, bool optDirection, float velRadius, ref Vector2 optimalHeading)
    {
        if (optDirection)
        {
            optimalHeading = velRadius * c.normalized;
        }
        else
        {
            //Clamp to velocity radius
            optimalHeading = Mathf.Clamp(c.magnitude, 0.0f, velRadius) * c.normalized;
        }
        
        List<HalfPlane2D> bounds = new List<HalfPlane2D>();

        foreach (HalfPlane2D halfPlane in halfPlanes)
        {
            if (Vector2.Dot(optimalHeading - halfPlane.p, halfPlane.n) < 0)
            {
                // Result does not satisfy constraint i. Compute new optimal result.
                Vector2 tempOptimalHeading = optimalHeading;
                if (!LinearProgram1D(c, halfPlane, bounds, optDirection, velRadius, ref optimalHeading))
                {
                    optimalHeading = tempOptimalHeading;
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
                    Vector2 v1 = -Vector2.Perpendicular(halfPlanes[i].n);
                    Vector2 v2 = -Vector2.Perpendicular(halfPlanes[j].n);
                    equidistantHalfPlanes.Add(new HalfPlane2D(Vector2.Perpendicular(v2 - v1).normalized, intersectionPoint));
                }

                Vector2 tempOptimalHeading = optimalHeading;
                if (!LinearProgram2D(halfPlanes[i].n, equidistantHalfPlanes, true, maxSpeed, ref optimalHeading))
                {
                    //This should in principle not happen. But due to floating point errors, it may.
                    optimalHeading = tempOptimalHeading;
                }

                /*
                float minMax = float.NegativeInfinity;
                foreach (HalfPlane2D hp in halfPlanes)
                {
                    minMax = Mathf.Max(minMax, -Vector2.Dot(hp.n, optimalHeading - hp.p));
                }
                Debug.Log(minMax);
                Debug.Log(optimalHeading);
                */
            }
        }

        /*
        float minMax = float.NegativeInfinity;
        foreach (HalfPlane2D hp in halfPlanes)
        {
            minMax = Mathf.Max(minMax, -Vector2.Dot(hp.n, optimalHeading - hp.p));
            Debug.Log(-Vector2.Dot(hp.n, optimalHeading - hp.p));
        }
        //Debug.Log(minMax);
        */
    }

    /*
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
                        halfPlanesTransformed.Add(new HalfPlane2D(-Vector2.Perpendicular(lineDirTransformed).normalized, linePosTransformed));
                    }
                }

                Vector2 vStar = Vector2.zero;
                Vector2 tempOptimalHeading = optimalHeading;
                if (LinearProgram2D(cTransformed, halfPlanesTransformed, false, maxSpeed, ref vStar))
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

        return optimalHeading;
    }
    */


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


        /*
        if (avoidanceLayer == agentB.avoidanceLayer)
        {
            ORCAHalfPlanes.Add(new HalfPlane2D(n, vOptA + 0.5f * u, vCenter.magnitude));
        }
        else if (avoidanceLayer < agentB.avoidanceLayer)
        {
            ORCAHalfPlanes.Add(new HalfPlane2D(n, vOptA + 0.0f * u, vCenter.magnitude));
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

