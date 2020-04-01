using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Succeeder : Decorator
{
    public Succeeder(BTNode child) : base(child)
    {

    }

    public override void OnBehave()
    {
        child.Behave();
    }

    public override void OnChildStopped(TaskResult result)
    {
        Stopped(TaskResult.SUCCESS);
    }

    public override void OnReset()
    {
    }
}
