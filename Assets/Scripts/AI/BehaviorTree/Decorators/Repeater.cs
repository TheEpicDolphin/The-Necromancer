using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repeater : Decorator
{
    public Repeater(BTNode child) : base(child)
    {

    }

    public override NodeStatus OnBehave(BehaviorState state)
    {
        NodeStatus status = child.Behave(state);
        if (status != NodeStatus.RUNNING)
        {
            Reset();
            child.Reset();
        }
        return NodeStatus.SUCCESS;
    }

    public override void OnReset()
    {
    }
}
