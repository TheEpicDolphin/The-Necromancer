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
    //protected string name;
    public string name;

    protected NodeState currentState = NodeState.INACTIVE;
    public BTNode parent;

    
    protected BehaviorTree tree;

    public BTNode(string name)
    {
        this.name = name;
    }

    public virtual void Behave()
    {
        currentState = NodeState.ACTIVE;
        tree.runningTaskNode = this;
        Debug.Log(name);

        OnBehave();

        ticks++;
        starting = false;
        
    }

    /// THIS ABSOLUTLY HAS TO BE THE LAST CALL IN YOUR FUNCTION, NEVER MODIFY
    /// ANY STATE AFTER CALLING Stopped !!!!
    protected virtual void Stopped(TaskResult result)
    {
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
        currentState = NodeState.INACTIVE;
        starting = true;
        ticks = 0;
        OnReset();
    }

    public abstract void OnReset();


    public virtual void ProvideMetaData(BehaviorTree tree)
    {
        this.tree = tree;
    }
}
