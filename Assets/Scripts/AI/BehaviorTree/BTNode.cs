using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeState
{
    ACTIVE,     //If this node is in the currently active branch
    INACTIVE    //Otherwise
}

public enum TaskResult
{
    FAILURE,
    SUCCESS
}

public abstract class BehaviorState
{

}

public abstract class BTNode
{
    public bool starting = true;
    protected bool debug = false;
    public int ticks = 0;

    protected NodeState currentState = NodeState.INACTIVE;
    public BTNode parent;
    protected BehaviorTree root;


    public virtual void Behave()
    {
        currentState = NodeState.ACTIVE;
        OnBehave();
        ticks++;
        starting = false;
    }

    /// THIS ABSOLUTLY HAS TO BE THE LAST CALL IN YOUR FUNCTION, NEVER MODIFY
    /// ANY STATE AFTER CALLING Stopped !!!!
    protected virtual void Stopped(TaskResult result)
    {
        currentState = NodeState.INACTIVE;
        if (this.parent != null)
        {
            this.Reset();
            this.parent.OnChildStopped(result);
        }
    }

    public abstract void OnChildStopped(TaskResult result);

    public abstract void OnBehave();

    public void Reset()
    {
        starting = true;
        ticks = 0;
        OnReset();
    }

    public abstract void OnReset();

}
