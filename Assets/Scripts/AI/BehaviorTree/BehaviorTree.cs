using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Event driven!
public class BehaviorTree
{
    BTNode rootNode;
    BTNode runningNode;

    List<BTNode> inOrderTraversal;
    
    public BehaviorTree(BTNode root)
    {
        this.rootNode = root;
        

    }


    
}

