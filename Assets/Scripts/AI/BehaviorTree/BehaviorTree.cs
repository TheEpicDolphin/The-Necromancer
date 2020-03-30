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

    protected List<BlackboardCondition> inOrderEventNodes;

    //OBSERVERS ARE RECALCULATED ON EACH TICK OF THE TREE
    
    
    public BehaviorTree(BTNode root)
    {
        this.rootNode = root;
        runningTaskNode = null;
    }

    
    public void Execute()
    {
        runningTaskNode.Behave();
    }

    void RunHigherPriorityListeningNode(string eventName)
    {
        //This works because observers are reset every tick of the behavior tree;
        List<BTNode> runningNodeBranch = new List<BTNode>();
        BTNode node = runningTaskNode;
        while (node != null)
        {
            runningNodeBranch.Add(node);
            node = node.parent;
        }

        List<BlackboardCondition> listeners = GetListeningNodesForEvent(eventName);
        BlackboardCondition highestPriorityListener = GetHighestPriorityListener(listeners);


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

        //I need to be careful here. I might run into Composites. This would be a problem. Figure something out!
        for (int i = 0; i < listenerNodeBranch.Count - d; i++)
        {
            //highestPriorityListener.
        }
        
        
    }
}

