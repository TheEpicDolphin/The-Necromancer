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
    
    
    public BehaviorTree(BTNode root)
    {
        this.rootNode = root;
        runningTaskNode = null;
    }

    
    public void Execute()
    {
        runningTaskNode.Behave();
    }

    void RunHigherPriorityListeningNode()
    {
        //Use O(n) binary parent search alg from GLMX interview lol
    }
}

