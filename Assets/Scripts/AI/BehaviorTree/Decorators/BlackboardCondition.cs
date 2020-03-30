using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BlackboardCondition : Decorator
{
    private string key;
    private object value;
    private Operator op;

    public BlackboardCondition(BTNode child, string key, Operator op, object value) : base(child)
    {
        this.op = op;
        this.key = key;
        this.value = value;
    }

    public override TaskResult OnBehave()
    {
        currentState = NodeState.STANDBY;
        StartObserving();
        if (IsConditionMet())
        {
            switch (child.Behave())
            {
                case TaskResult.RUNNING:
                    //This makes sure that only the node that is actually running is running
                    currentState = NodeState.STANDBY;
                    return NodeStatus.STANDBY;

                case TaskResult.STANDBY:
                    return NodeStatus.STANDBY;

                case TaskResult.SUCCESS:
                    return TaskResult.FAILURE;

                case TaskResult.FAILURE:
                    return TaskResult.SUCCESS;
            }
        }
        return TaskResult.FAILURE;
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

        if (!this.root.blackboard.Isset(key))
        {
            return op == Operator.IS_NOT_SET;
        }

        object o = this.root.blackboard.Get(key);

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

    void Evaluate()
    {
        if (IsConditionMet())
        {
            root.RunHigherPriorityListeningNode();
        }
    }

    public override void OnReset()
    {
    }
}
