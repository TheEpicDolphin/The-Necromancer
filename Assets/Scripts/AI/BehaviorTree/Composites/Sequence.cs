using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sequence : Composite
{
    int currentChild = 0;

    public Sequence(string compositeName, params BTNode[] nodes) : base(compositeName, nodes)
    {

    }

    public override void OnBehave()
    {
        children[currentChild].Behave();
    }

    public override void OnChildStopped(TaskResult result)
    {
        switch (result)
        {
            case TaskResult.SUCCESS:
                currentChild++;
                break;

            case TaskResult.FAILURE:
                Stopped(TaskResult.FAILURE);
                break;      
        }

        if (currentChild >= children.Count)
        {
            Stopped(TaskResult.SUCCESS);
        }
        else if (result == TaskResult.SUCCESS)
        {
            // if we succeeded, don't wait for the next tick to process the next child
            OnBehave();
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
