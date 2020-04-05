using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Service : Decorator
{

    Action action;
    float interval;
    Coroutine serviceCoroutine;
    MonoBehaviour coroutineRunner;

    public Service(string name, MonoBehaviour coroutineRunner, float interval, Action action, BTNode child) : base(name, child)
    {
        this.interval = interval;
        this.action = action;
        this.coroutineRunner = coroutineRunner;
    }

    public override void OnBehave()
    {
        Debug.Log("how");
        serviceCoroutine = coroutineRunner.StartCoroutine(ServiceCoroutine());
        child.Behave();
    }

    IEnumerator ServiceCoroutine()
    {
        while (true)
        {
            action();
            yield return new WaitForSeconds(this.interval);
        }
    }

    public override void OnChildStopped(TaskResult result)
    {
        switch (result)
        {
            case TaskResult.SUCCESS:
                Stopped(TaskResult.SUCCESS);
                break;

            case TaskResult.FAILURE:
                Stopped(TaskResult.FAILURE);
                break;
        }

    }

    public override void OnReset()
    {
        coroutineRunner.StopCoroutine(serviceCoroutine);
    }
}
