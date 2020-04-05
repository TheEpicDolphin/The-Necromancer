using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Decorator : BTNode
{
    protected BTNode child;

    public Decorator(string name, BTNode node) : base(name)
    {
        child = node;
        child.parent = this;
    }

    public override void ProvideMetaData(BehaviorTree tree)
    {
        base.ProvideMetaData(tree);
        child.ProvideMetaData(tree);
    }
}
