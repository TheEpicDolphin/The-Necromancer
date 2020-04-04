using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repeater : Decorator
{
    public Repeater(string name, BTNode child) : base(name, child)
    {

    }

    public override void OnBehave()
    {
        child.Behave();
    }

    public override void OnChildStopped(TaskResult result)
    {
        Reset();
        child.Reset();
        Stopped(TaskResult.SUCCESS);
    }

    public override void OnReset()
    {
    }
}
