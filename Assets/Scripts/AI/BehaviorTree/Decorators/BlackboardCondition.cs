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


    public override NodeStatus OnBehave(BehaviorState state)
    {
        if ()
        {
            switch (child.Behave(state))
            {
                case NodeStatus.RUNNING:
                    return NodeStatus.RUNNING;

                case NodeStatus.SUCCESS:
                    return NodeStatus.FAILURE;

                case NodeStatus.FAILURE:
                    return NodeStatus.SUCCESS;
            }
        }
        return NodeStatus.FAILURE;
    }

    private void OnValueChanged(Blackboard.Type type, object newValue)
    {
        Evaluate();
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

    protected void Evaluate()
    {
        if (IsActive && !IsConditionMet())
        {
            if (stopsOnChange == Stops.SELF || stopsOnChange == Stops.BOTH || stopsOnChange == Stops.IMMEDIATE_RESTART)
            {
                // Debug.Log( this.key + " stopped self ");
                this.Stop();
            }
        }
        else if (!IsActive && IsConditionMet())
        {
            if (stopsOnChange == Stops.LOWER_PRIORITY || stopsOnChange == Stops.BOTH || stopsOnChange == Stops.IMMEDIATE_RESTART || stopsOnChange == Stops.LOWER_PRIORITY_IMMEDIATE_RESTART)
            {
                // Debug.Log( this.key + " stopped other ");
                Container parentNode = this.ParentNode;
                Node childNode = this;
                while (parentNode != null && !(parentNode is Composite))
                {
                    childNode = parentNode;
                    parentNode = parentNode.ParentNode;
                }
                Assert.IsNotNull(parentNode, "NTBtrStops is only valid when attached to a parent composite");
                Assert.IsNotNull(childNode);
                if (parentNode is Parallel)
                {
                    Assert.IsTrue(stopsOnChange == Stops.IMMEDIATE_RESTART, "On Parallel Nodes all children have the same priority, thus Stops.LOWER_PRIORITY or Stops.BOTH are unsupported in this context!");
                }

                if (stopsOnChange == Stops.IMMEDIATE_RESTART || stopsOnChange == Stops.LOWER_PRIORITY_IMMEDIATE_RESTART)
                {
                    if (isObserving)
                    {
                        isObserving = false;
                        StopObserving();
                    }
                }

                ((Composite)parentNode).StopLowerPriorityChildrenForChild(childNode, stopsOnChange == Stops.IMMEDIATE_RESTART || stopsOnChange == Stops.LOWER_PRIORITY_IMMEDIATE_RESTART);
            }
        }
    }

    public override void OnReset()
    {
    }
}
