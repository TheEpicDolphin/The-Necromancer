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
    private bool isNotifiyng = false;
    private Dictionary<string, List<System.Action<Type, object>>> addObservers = new Dictionary<string, List<System.Action<Type, object>>>();
    private Dictionary<string, List<System.Action<Type, object>>> removeObservers = new Dictionary<string, List<System.Action<Type, object>>>();
    private List<Notification> notifications = new List<Notification>();
    private List<Notification> notificationsDispatch = new List<Notification>();
    private Blackboard parentBlackboard;
    private HashSet<Blackboard> children = new HashSet<Blackboard>();

    public object this[string key]
    {
        get
        {
            return Get(key);
        }
        set
        {
            Set(key, value);
        }
    }

    public void Set(string key)
    {
        if (!Isset(key))
        {
            Set(key, null);
        }
    }

    public void Set(string key, object value)
    {
        if (this.parentBlackboard != null && this.parentBlackboard.Isset(key))
        {
            this.parentBlackboard.Set(key, value);
        }
        else
        {
            if (!this.data.ContainsKey(key))
            {
                this.data[key] = value;
                this.notifications.Add(new Notification(key, Type.ADD, value));
                this.clock.AddTimer(0f, 0, NotifiyObservers);
            }
            else
            {
                if ((this.data[key] == null && value != null) || (this.data[key] != null && !this.data[key].Equals(value)))
                {
                    this.data[key] = value;
                    this.notifications.Add(new Notification(key, Type.CHANGE, value));
                    this.clock.AddTimer(0f, 0, NotifiyObservers);
                }
            }
        }
    }

    private void NotifiyObservers()
    {
        if (notifications.Count == 0)
        {
            return;
        }

        notificationsDispatch.Clear();
        notificationsDispatch.AddRange(notifications);
        foreach (Blackboard child in children)
        {
            child.notifications.AddRange(notifications);
            child.clock.AddTimer(0f, 0, child.NotifiyObservers);
        }
        notifications.Clear();

        isNotifiyng = true;
        foreach (Notification notification in notificationsDispatch)
        {
            if (!this.observers.ContainsKey(notification.key))
            {
                //                Debug.Log("1 do not notify for key:" + notification.key + " value: " + notification.value);
                continue;
            }

            List<System.Action<Type, object>> observers = GetObserverList(this.observers, notification.key);
            foreach (System.Action<Type, object> observer in observers)
            {
                if (this.removeObservers.ContainsKey(notification.key) && this.removeObservers[notification.key].Contains(observer))
                {
                    continue;
                }
                observer(notification.type, notification.value);
            }
        }

        foreach (string key in this.addObservers.Keys)
        {
            GetObserverList(this.observers, key).AddRange(this.addObservers[key]);
        }
        foreach (string key in this.removeObservers.Keys)
        {
            foreach (System.Action<Type, object> action in removeObservers[key])
            {
                GetObserverList(this.observers, key).Remove(action);
            }
        }
        this.addObservers.Clear();
        this.removeObservers.Clear();

        isNotifiyng = false;
    }
}
