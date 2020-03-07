using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : Melee
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Swing()
    {
        //Enable hurtBox to cause damage
        hurtBox.enabled = true;
        //Play swinging animation
        StartCoroutine(AttackCoroutine());
    }

    IEnumerator AttackCoroutine()
    {
        //Wait for animation to finish

        yield return null;
        //Finished swinging. Disable hurtBox again
        hurtBox.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {

    }
}
