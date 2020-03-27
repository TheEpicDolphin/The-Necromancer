using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeStatus
{
    FAILURE,
    SUCCESS,
    RUNNING
}

public abstract class BehaviorState
{

}

public abstract class BehaviorTreeNode
{
    public bool starting = true;
    protected bool debug = false;
    public int ticks = 0;

    public virtual NodeStatus Behave(BehaviorState state)
    {
        NodeStatus status = OnBehave(state);

        string result = "unknown";
        switch (status)
        {
            case NodeStatus.FAILURE:
                result = "failure";
                break;
            case NodeStatus.SUCCESS:
                result = "success";
                break;
            case NodeStatus.RUNNING:
                result = "running";
                break;
        }
        Debug.Log("Behaving: " + GetType().Name + " - " + result);

        ticks++;
        starting = false;

        if (status != NodeStatus.RUNNING)
        {
            Reset();
        }
            

        return status;
    }

    public abstract NodeStatus OnBehave(BehaviorState state);

    public void Reset()
    {
        starting = true;
        ticks = 0;
        OnReset();
    }

    public abstract void OnReset();

}
