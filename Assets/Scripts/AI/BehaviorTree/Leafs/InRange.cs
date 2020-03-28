using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRange : Leaf
{
    public float distanceThreshold;

    public InRange(float threshold)
    {
        distanceThreshold = threshold;
    }
    public override NodeStatus OnBehave(BehaviorState state)
    {
        Context context = (Context)state;

        if (context.ai.target != null && Vector3.Distance(context.ai.transform.position, context.ai.target.position) < distanceThreshold)
        {
            return NodeStatus.SUCCESS;
        }
        return NodeStatus.FAILURE;
    }

    public override void OnReset()
    {
    }
}

