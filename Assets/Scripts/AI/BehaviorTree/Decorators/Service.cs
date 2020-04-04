using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Service : Decorator
{
    
    private class CoroutineRunner : MonoBehaviour {
        private static CoroutineRunner _instance;
        public static CoroutineRunner Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }
    }

    Action action;
    float interval;
    Coroutine serviceCoroutine;

    public Service(float interval, Action action, BTNode child) : base(child)
    {
        this.interval = interval;
        this.action = action;
    }

    public override void OnBehave()
    {
        serviceCoroutine = CoroutineRunner.Instance.StartCoroutine(ServiceCoroutine());
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
        CoroutineRunner.Instance.StopCoroutine(serviceCoroutine);
    }
}
