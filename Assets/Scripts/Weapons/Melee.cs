using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour
{
    protected BoxCollider hurtBox;
    // Start is called before the first frame update
    void Start()
    {
        hurtBox = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void Swing()
    {

    }
}
