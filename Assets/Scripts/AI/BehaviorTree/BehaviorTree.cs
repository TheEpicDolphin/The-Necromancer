using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Event driven!
public class BehaviorTree
{
    public Blackboard blackboard;
    protected Root rootNode;
    public BTNode runningTaskNode;

    //OBSERVERS ARE RECALCULATED ON EACH TICK OF THE TREE
    public BehaviorTree(Root root)
    {
        rootNode = root;
        runningTaskNode = root;
        blackboard = new Blackboard();

        rootNode.ProvideMetaData(this);
    }

    
    public void Execute()
    {
        //Debug.Log(runningTaskNode.GetType());
        runningTaskNode.Behave();
    }

    public void NotifyListeningNodesForEvent(string eventName)
    {
        //This works because observers are reset every tick of the behavior tree;
        List<BTNode> runningNodeBranch = new List<BTNode>();
        BTNode node = runningTaskNode;
        while (node != null)
        {
            runningNodeBranch.Add(node);
            node = node.parent;
        }

        List<BlackboardCondition> tempObserverQueue = new List<BlackboardCondition>();
        BlackboardCondition highestPriorityListener = null;
        //Debug.Log(blackboard.observerQueue.Count);
        foreach (BlackboardCondition observer in blackboard.observerQueue)
        {
            if (observer.key == eventName)
            {
                highestPriorityListener = observer;
                break;
            }
            tempObserverQueue.Add(observer);
        }
        if (highestPriorityListener == null)
        {
            return;
        }
        blackboard.observerQueue = tempObserverQueue;

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

        Debug.Log("the fuck");
        //Run the intersection node of the two branches
        listenerNodeBranch[listenerNodeBranch.Count - d - 1].Behave();
    }

    

}

