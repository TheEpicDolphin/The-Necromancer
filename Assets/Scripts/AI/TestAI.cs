using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAI : NavAgent
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();

        Vector3 meshExtents = GetComponent<MeshRenderer>().bounds.extents;
        radius = meshExtents.x;
    }

    public override void MoveAgent(Vector3 heading)
    {
        if (!overrideNav)
        {
            Vector3 curPos = new Vector3(transform.position.x, 0.1f, transform.position.z);
            rb.MovePosition(curPos + heading * Time.deltaTime);
            //rb.AddForce(heading * Time.deltaTime);

            //Smooth movement
            if (pathPoints.Count == 1)
            {
                tempPreferredHeading = heading;
                Vector3.SmoothDamp(transform.position, pathPoints[0], ref tempPreferredHeading, 0.3f, speed);
            }
            else
            {
                tempPreferredHeading = Vector3.Lerp(heading, speed * (pathPoints[0] - curPos).normalized, 5.0f * Time.deltaTime);
                //desiredHeading = speed * (pathPoints[0] - curPos).normalized;
            }


            Debug.DrawLine(curPos, curPos + tempPreferredHeading, Color.cyan);
        }
        else
        {
            tempPreferredHeading = rb.velocity;
        }


    }
}
