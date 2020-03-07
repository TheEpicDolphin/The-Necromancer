using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBox : MonoBehaviour
{
    Collider hurtCollider;
    // Start is called before the first frame update
    void Start()
    {
        hurtCollider = GetComponent<Collider>();
    }


    private void OnTriggerEnter(Collider other)
    {
        
    }

}
