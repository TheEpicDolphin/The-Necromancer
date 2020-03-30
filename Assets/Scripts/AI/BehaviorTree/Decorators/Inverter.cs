using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inverter : Decorator
{
    public Inverter(BTNode child) : base(child)
    {

    }

    public override void OnBehave()
    {
        child.Behave();
    }

    public override void OnChildStopped(TaskResult result)
    {
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
