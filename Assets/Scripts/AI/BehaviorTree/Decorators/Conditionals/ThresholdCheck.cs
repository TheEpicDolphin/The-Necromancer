using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ThresholdCheck<T> : Conditional where T : IComparable
{
    T threshold;

    public ThresholdCheck(BTNode child) : base(child)
    {

    }


}
