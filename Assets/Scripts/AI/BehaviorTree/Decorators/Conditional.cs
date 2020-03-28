using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conditional : Decorator
{
    protected float threshold;

    public Conditional(BTNode child) : base(child)
    {

    }

    public override NodeStatus OnBehave(BehaviorState state)
    {
        if ()
        {
            switch (child.Behave(state))
            {
                case NodeStatus.RUNNING:
                    return NodeStatus.RUNNING;

                case NodeStatus.SUCCESS:
                    return NodeStatus.FAILURE;

                case NodeStatus.FAILURE:
                    return NodeStatus.SUCCESS;
            }
        }
        return NodeStatus.FAILURE;
    }

    public override void OnReset()
    {
    }
}
