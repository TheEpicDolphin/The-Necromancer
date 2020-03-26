using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAI : NavAgent
{
    // Start is called before the first frame update
    new void Start()
    {
        Vector3 meshExtents = GetComponent<MeshRenderer>().bounds.extents;
        radius = meshExtents.x;
        base.Start();
    }

    private void FixedUpdate()
    {
        MoveAgent();
    }

    public override void MoveAgent()
    {
        if (!overrideNav)
        {
            Vector3 curPos = new Vector3(transform.position.x, 0.0f, transform.position.z);
            rb.velocity = optimalVelocity;
            //rb.MovePosition(curPos + optimalVelocity * Time.fixedDeltaTime);
            //Smooth movement
            if (pathPoints.Count == 1)
            {
                desiredHeading = optimalVelocity;
                Vector3.SmoothDamp(transform.position, pathPoints[0], ref desiredHeading, 0.3f, maxSpeed);
            }
            else
            {
                //desiredHeading = Vector3.Lerp(optimalVelocity, maxSpeed * (pathPoints[0] - curPos).normalized, 5.0f * Time.fixedDeltaTime);
                desiredHeading = optimalVelocity;
            }
            Debug.DrawLine(curPos, curPos + desiredHeading, Color.cyan);
        }
        else
        {
            desiredHeading = rb.velocity;
        }


    }
}
