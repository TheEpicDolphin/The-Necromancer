using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : Composite
{
    int currentChild = 0;

    public Sequence(string compositeName, params BehaviorTreeNode[] nodes) : base(compositeName, nodes)
    {

    }

    public override NodeStatus OnBehave(BehaviorState state)
    {
        NodeStatus status = children[currentChild].Behave(state);

        switch (status)
        {
            case NodeStatus.SUCCESS:
                currentChild++;
                break;

            case NodeStatus.FAILURE:
                return NodeStatus.FAILURE;
        }

        if (currentChild >= children.Count)
        {
            return NodeStatus.SUCCESS;
        }
        else if (status == NodeStatus.SUCCESS)
        {
            // if we succeeded, don't wait for the next tick to process the next child
            return OnBehave(state);
        }

        return NodeStatus.RUNNING;
    }

    public override void OnReset()
    {
        currentChild = 0;
        foreach (BehaviorTreeNode child in children)
        {
            child.Reset();
        }
    }
}
