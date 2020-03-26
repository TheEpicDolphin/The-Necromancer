﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : NavAgent
{
    HealthBar healthBar;
    Animator animator;

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

        MoveAgent();
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
}
