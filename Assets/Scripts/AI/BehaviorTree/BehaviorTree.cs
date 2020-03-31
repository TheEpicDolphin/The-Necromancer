using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Event driven!
public class BehaviorTree
{
    public Blackboard blackboard;
    protected BTNode rootNode;
    public Task runningTaskNode;

    //private Dictionary<string, List<System.Action<Type, object>>> observers = new Dictionary<string, List<System.Action<Type, object>>>();
    private Dictionary<string, List<BlackboardCondition>> observers = new Dictionary<string, List<BlackboardCondition>>();

    //OBSERVERS ARE RECALCULATED ON EACH TICK OF THE TREE
    public BehaviorTree(BTNode root)
    {
        this.rootNode = root;
        runningTaskNode = null;
    }

    
    public void Execute()
    {
        observers = new Dictionary<string, List<BlackboardCondition>>();
        runningTaskNode.Behave();
    }

    void NotifyListeningNodesForEvent(string eventName)
    {
        //This works because observers are reset every tick of the behavior tree;
        List<BTNode> runningNodeBranch = new List<BTNode>();
        BTNode node = runningTaskNode;
        while (node != null)
        {
            runningNodeBranch.Add(node);
            node = node.parent;
        }

        List<BlackboardCondition> listeners = observers[eventName];
        BlackboardCondition highestPriorityListener = listeners[0];


        List<BTNode> listenerNodeBranch = new List<BTNode>();
        node = highestPriorityListener;
        while (node != null)
        {
            listenerNodeBranch.Add(node);
            node = node.parent;
        }

        int d = Mathf.Max(runningNodeBranch.Count, listenerNodeBranch.Count) - Mathf.Min(runningNodeBranch.Count, listenerNodeBranch.Count);
        for(int i = 0; i < runningNodeBranch.Count - d; i++)
        {
            runningNodeBranch[i].Reset();
        }

        for (int i = 0; i < listenerNodeBranch.Count - d - 1; i++)
        {
            listenerNodeBranch[i].Reset();
        }

        //Run the intersection node of the two branches
        listenerNodeBranch[listenerNodeBranch.Count - d - 1].Behave();
    }

    public void AddObserver(string eventName, BlackboardCondition conditionNode)
    {

    }
}

