using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackboard
{

    public enum Type
    {
        ADD,
        REMOVE,
        CHANGE
    }

    private Dictionary<string, object> data = new Dictionary<string, object>();
    private Dictionary<string, List<System.Action<Type, object>>> observers = new Dictionary<string, List<System.Action<Type, object>>>();

    public object this[string key]
    {
        get
        {
            return this.data[key];
        }
        set
        {
            this.data[key] = value;
        }
    }

    public T Get<T>(string key)
    {
        object value = this.data[key];
        if (value == null)
        {
            return default(T);
        }
        return (T)value;
    }

    public bool IsSet(string key)
    {
        return this.data.ContainsKey(key);
    }

}
