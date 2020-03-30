using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Composite : BTNode
{
    protected List<BTNode> children = new List<BTNode>();
    public string compositeName;

    public Composite(string name, params BTNode[] nodes)
    {
        compositeName = name;
        children.AddRange(nodes);
        foreach(BTNode child in children)
        {
            child.parent = this;
            
        }
    }

    public override void Behave()
    {
        bool shouldLog = debug && ticks == 0 ? true : false;
        if (shouldLog)
        {
            Debug.Log("Running behaviour list: " + compositeName);
        }

        base.Behave();
    }


}
