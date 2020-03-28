using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ZombieBehaviorTree : BehaviorTree
{
    public UnityEvent deathEvent = new UnityEvent();
    public UnityEvent knockbackEvent = new UnityEvent();


    //knockbackEvent.AddListener(KnockbackFunc);
    //knockbackEvent.Invoke();
    
}
