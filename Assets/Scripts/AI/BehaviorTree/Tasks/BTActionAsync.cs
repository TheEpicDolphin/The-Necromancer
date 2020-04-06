using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Always returns true
public class BTActionAsync : Task
{
    IEnumerator asyncAction;
    MonoBehaviour coroutineRunner;

    public BTActionAsync(string name, MonoBehaviour coroutineRunner, IEnumerator asyncAction) : base(name)
    {
        this.asyncAction = asyncAction;
    }

    public IEnumerator WaitForThreadJoin()
    {
        yield return coroutineRunner.StartCoroutine(asyncAction);
        Stopped(TaskResult.SUCCESS);
    }

    public override void OnBehave()
    {
        coroutineRunner.StartCoroutine(WaitForThreadJoin());
    }

    public override void OnChildStopped(TaskResult result)
    {

    }

    public override void OnReset()
    {

    }
}
