using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wait : Task
{
    public Wait(string name) : base(name)
    {
        
    }

    public override void OnBehave()
    {
        //Do nothing
    }

    public override void OnChildStopped(TaskResult result)
    {
        //Has no children
    }

    public override void OnReset()
    {
        //Do nothing
    }
}
