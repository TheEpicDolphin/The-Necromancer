using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    public Transform target;
    private Rigidbody rb;

    public float visionRadius;

    public float closeRadius;

    public float drag;

    public float separationStrength;

    public float matchStrength;

    public float groupStrength;

    public float homingStrength;

    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MovementUpdate()
    {

        Collider[] closeColliders = Physics.OverlapSphere(transform.position, visionRadius);
        List<Zombie> hordemates = new List<Zombie>();
        for (int i = 0; i < closeColliders.Length; i++)
        {
            Collider c = closeColliders[i];
            Zombie z = c.gameObject.GetComponent<Zombie>();
            if (z && z != this) hordemates.Add(z);
        }

        //        if (hordemates.Count == 0) // TODO: fix so that will go to player if haven't got mates
        //        {
        //            return;
        //        }
        int totClose = 0;
        Vector3 closeDir = new Vector3();
        Vector3 totPos = new Vector3();
        Vector3 totVel = new Vector3();
        foreach (Zombie hordemate in hordemates)
        {
            if (Vector3.Distance(transform.position, hordemate.transform.position) < closeRadius)
            {
                totClose++;
                float dist = Vector3.Distance(transform.position, hordemate.transform.position);
                closeDir += (transform.position - hordemate.transform.position) /
                            (dist * dist);
            }

            totPos += hordemate.transform.position - transform.position;
            totVel += hordemate.GetVelocity();
        }

        if (totClose > 0) closeDir /= totClose;
        Vector3 currVelocityDir = rb.velocity;
        if (rb.velocity.magnitude > 0) currVelocityDir /= currVelocityDir.magnitude;
        Vector3 playerDir = target.position - transform.position;
        //
        //        Debug.Log("things:");
        //        Debug.Log(totVel);
        //        Debug.Log(totClose);
        //        Debug.Log(currVelocityDir);
        //        Debug.Log(totPos);
        //        Debug.Log(closeDir);
        Vector3 force = -drag * rb.velocity * (rb.velocity.magnitude - speed) +
                        separationStrength * closeDir +
                        matchStrength * (totVel.normalized - currVelocityDir) +
                        groupStrength * totPos.normalized +
                        homingStrength * playerDir.normalized;

        rb.AddForce(force);
    }

    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }
}
