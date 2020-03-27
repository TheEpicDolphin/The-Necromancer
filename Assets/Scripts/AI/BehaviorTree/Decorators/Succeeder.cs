using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Succeeder : Decorator
{
    public Succeeder(BehaviorTreeNode child) : base(child)
    {

    }

    public override NodeStatus OnBehave(BehaviorState state)
    {
        NodeStatus status = child.Behave(state);

        if (status == NodeStatus.RUNNING)
        {
            return NodeStatus.RUNNING;
        }
            
        return NodeStatus.SUCCESS;
    }

    public override void OnReset()
    {
    }
}
