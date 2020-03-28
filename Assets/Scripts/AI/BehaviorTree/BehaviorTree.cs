using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Event driven!
public class BehaviorTree
{
    protected BTNode rootNode;
    protected BTNode runningNode;

    protected List<BTNode> inOrderEventNodes;
    Dictionary<string, Conditional> eventMap;
    
    public BehaviorTree(BTNode root)
    {
        this.rootNode = root;
    }

    
    public void InvokeEvent(string eventNodeName)
    {
        
    }
}

