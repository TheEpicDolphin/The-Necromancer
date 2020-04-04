﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Succeeder : Decorator
{
    public Succeeder(string name, BTNode child) : base(name, child)
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
