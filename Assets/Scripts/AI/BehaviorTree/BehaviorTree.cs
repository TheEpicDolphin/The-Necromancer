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
        int runningTaskNodeDepth = 0;
        BTNode node = runningTaskNode;
        while (node != null)
        {
            runningTaskNodeDepth += 1;
            node = node.parent;
        }

        List<BlackboardCondition> tempObserverQueue = new List<BlackboardCondition>();
        BlackboardCondition highestPriorityListener = null;
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

        int listenerNodeDepth = 0;
        node = highestPriorityListener;
        while (node != null)
        {
            listenerNodeDepth += 1;
            node = node.parent;
        }

        
        BTNode runningBranchNode = runningTaskNode;
        BTNode listenerBranchNode = highestPriorityListener;
        int d = Mathf.Max(runningTaskNodeDepth, listenerNodeDepth) - Mathf.Min(runningTaskNodeDepth, listenerNodeDepth);
        if (runningTaskNodeDepth > listenerNodeDepth)
        {
            for (int i = 0; i < d; i++)
            {
                runningBranchNode.Reset();
                runningBranchNode = runningBranchNode.parent;
            }
        }
        else
        {
            for (int i = 0; i < d; i++)
            {
                listenerBranchNode.Reset();
                listenerBranchNode = listenerBranchNode.parent;
            }
        }

        while (runningBranchNode != listenerBranchNode)
        {
            runningBranchNode.Reset();
            runningBranchNode = runningBranchNode.parent;

            listenerBranchNode.Reset();
            listenerBranchNode = listenerBranchNode.parent;
        }

        runningBranchNode.Reset();
        runningBranchNode.Behave();
        
    }



}

