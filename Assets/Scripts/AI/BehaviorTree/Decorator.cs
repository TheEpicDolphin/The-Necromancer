using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Decorator : BTNode
{
    protected BTNode child;

    public Decorator(BTNode node)
    {
        child = node;
    }
}
