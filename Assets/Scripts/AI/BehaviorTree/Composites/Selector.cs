using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : Composite
{
    int currentChild = 0;

    public Selector(string compositeName, params BTNode[] nodes) : base(compositeName, nodes)
    {

    }

    public override void OnBehave()
    {
        if (currentChild >= children.Count)
        {
            Stopped(TaskResult.FAILURE);
            return;
        }

        children[currentChild].Behave();
    }

    public override void OnChildStopped(TaskResult result)
    {

        switch (result)
        {
            case TaskResult.SUCCESS:
                Stopped(TaskResult.SUCCESS);
                break;
            case TaskResult.FAILURE:
                currentChild++;
                // If we failed, immediately process the next child
                OnBehave();
                break;
        }
        
    }

    public override void OnReset()
    {
        currentChild = 0;
        foreach (BTNode child in children)
        {
            child.Reset();
        }
    }
}
