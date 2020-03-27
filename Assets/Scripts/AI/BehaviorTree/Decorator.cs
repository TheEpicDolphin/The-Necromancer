using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Decorator : BehaviorTreeNode
{
    protected BehaviorTreeNode child;

    public Decorator(BehaviorTreeNode node)
    {
        child = node;
    }
}
