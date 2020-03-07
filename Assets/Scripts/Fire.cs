using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : Magic
{
    float coolDown = 0.5f;
    float nextAttackTime;
    // Start is called before the first frame update
    void Start()
    {
        nextAttackTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Cast()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit) && Time.time > nextAttackTime)
        {
            nextAttackTime = Time.time + coolDown;
            Transform objectHit = hit.transform;

            //Play shooting fire animation. Maybe use coroutine

            //Instantiate fireball
            GameObject fireball = (GameObject)Instantiate(Resources.Load("Prefabs/fireball", typeof(GameObject)));
            fireball.transform.position = transform.position + new Vector3(0.0f, 2.0f, 0.0f);
            fireball.transform.forward = -Camera.main.transform.forward;
            fireball.transform.up = Camera.main.transform.up;
            Rigidbody fireballRigidbody = fireball.GetComponent<Rigidbody>();
            Vector3 shootDir = hit.point - transform.position;
            fireballRigidbody.velocity = 30.0f * shootDir.normalized;
            Destroy(fireball, 3.0f);
        }


    }
}
