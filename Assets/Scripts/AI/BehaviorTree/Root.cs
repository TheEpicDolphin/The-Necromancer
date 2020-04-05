using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : Decorator
{
    public Root(string name, BTNode child) : base(name, child)
    {

    }

    public override void OnBehave()
    {
        child.Behave();
    }

    public override void OnChildStopped(TaskResult result)
    {
        tree.runningTaskNode = this;
        switch (result)
        {
            case TaskResult.SUCCESS:
                Stopped(TaskResult.FAILURE);
                break;

            case TaskResult.FAILURE:
                Stopped(TaskResult.SUCCESS);
                break;
        }

    }

    public override void OnReset()
    {
    }
}
