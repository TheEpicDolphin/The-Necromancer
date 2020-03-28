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
    }

    public override NodeStatus Behave(BehaviorState state)
    {
        bool shouldLog = debug && ticks == 0 ? true : false;
        if (shouldLog)
        {
            Debug.Log("Running behaviour list: " + compositeName);
        }    

        NodeStatus status = base.Behave(state);

        if (debug && status != NodeStatus.RUNNING)
        {
            Debug.Log("Behaviour list " + compositeName + " returned: " + status.ToString());
        }
            
        return status;
    }

}
