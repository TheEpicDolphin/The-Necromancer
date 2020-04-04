using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BTAction : Task
{
    Func<TaskResult> action;

    public BTAction(string name, Func<TaskResult> action) : base(name)
    {
        this.action = action;
    }

    public override void OnBehave()
    {
        Stopped(action());
    }

    public override void OnChildStopped(TaskResult result)
    {
        
    }

    public override void OnReset()
    {
        
    }
}
