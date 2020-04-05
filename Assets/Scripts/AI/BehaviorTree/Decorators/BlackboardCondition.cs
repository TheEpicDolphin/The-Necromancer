using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackboardCondition : Decorator
{
    public string key;
    private object value;
    private Operator op;
    private Blackboard blackboard;

    public BlackboardCondition(string name, string key, Operator op, object value, BTNode child) : base(name, child)
    {
        this.op = op;
        this.key = key;
        this.value = value;
    }

    public override void OnBehave()
    {
        blackboard.AddObserver(this);
        if (IsConditionMet())
        {
            child.Behave();
        }
        else
        {
            Stopped(TaskResult.FAILURE);
        }
        
    }

    public override void ProvideMetaData(BehaviorTree root)
    {
        blackboard = root.blackboard;
        base.ProvideMetaData(root);
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

    private void OnValueChanged(Blackboard.Type type, object newValue)
    {
        OnBehave();
    }

    bool IsConditionMet()
    {
        if (op == Operator.ALWAYS_TRUE)
        {
            return true;
        }

        if (!this.blackboard.IsSet(key))
        {
            return op == Operator.IS_NOT_SET;
        }

        object o = this.blackboard[key];

        switch (this.op)
        {
            case Operator.IS_SET: return true;
            case Operator.IS_EQUAL: return object.Equals(o, value);
            case Operator.IS_NOT_EQUAL: return !object.Equals(o, value);

            case Operator.IS_GREATER_OR_EQUAL:
                if (o is float)
                {
                    return (float)o >= (float)this.value;
                }
                else if (o is int)
                {
                    return (int)o >= (int)this.value;
                }
                else
                {
                    Debug.LogError("Type not compareable: " + o.GetType());
                    return false;
                }

            case Operator.IS_GREATER:
                if (o is float)
                {
                    return (float)o > (float)this.value;
                }
                else if (o is int)
                {
                    return (int)o > (int)this.value;
                }
                else
                {
                    Debug.LogError("Type not compareable: " + o.GetType());
                    return false;
                }

            case Operator.IS_LESS_OR_EQUAL:
                if (o is float)
                {
                    return (float)o <= (float)this.value;
                }
                else if (o is int)
                {
                    return (int)o <= (int)this.value;
                }
                else
                {
                    Debug.LogError("Type not compareable: " + o.GetType());
                    return false;
                }

            case Operator.IS_LESS:
                if (o is float)
                {
                    return (float)o < (float)this.value;
                }
                else if (o is int)
                {
                    return (int)o < (int)this.value;
                }
                else
                {
                    Debug.LogError("Type not compareable: " + o.GetType());
                    return false;
                }

            default: return false;
        }
    }

    /*
    void Evaluate()
    {
        if (IsConditionMet())
        {
            root.RunHigherPriorityListeningNode();
        }
    }
    */

    public override void OnReset()
    {
        //root.RemoveObserver(this, key);
    }
}
