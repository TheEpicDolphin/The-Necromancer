using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    public float attackRange;
    public float speed;
    public GameObject hitDetectorPrefab;

    private Rigidbody rb;

    private HitDetector detector;
    // Start is called before the first frame update
    void Start()
    {
        detector = Instantiate(hitDetectorPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1)).GetComponent<HitDetector>();
        detector.gameObject.transform.SetParent(this.gameObject.transform, false);
        detector.gameObject.transform.localScale = new Vector3(attackRange, 1, attackRange);

        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }

        int upness = Upness();
        int rightness = Rightness();
        Vector3 movement = new Vector3(rightness, 0, upness);
        rb.velocity = speed * movement;

        if (upness != 0 || rightness != 0)
        {
            detector.transform.localRotation = GetQuaternion(rightness, upness);
        }
    }

    void Attack()
    {
        foreach (Collider c in detector.GetColliding())
        {

            if (c.gameObject.GetComponent<Zombie>()) Destroy(c.gameObject); // replace with calling enemy's check attack fn
        }
    }

    private int Upness()
    {
        return (Input.GetKey(KeyCode.W)?1:0) - (Input.GetKey(KeyCode.S)?1:0);
    }

    private int Rightness()
    {
        return (Input.GetKey(KeyCode.D)?1:0) - (Input.GetKey(KeyCode.A)?1:0);
    }

    private static Quaternion GetQuaternion(int rightness, int upness)
    {
        int rot_id = 3 * rightness + upness;
        int rotations = 0;
        switch (rot_id)
        {
            case -4:
                rotations = -3;
                break;
            case -3:
                rotations = 4;
                break;
            case -2:
                rotations = 3;
                break;
            case -1:
                rotations = -2;
                break;
            case 0:
                rotations = 0; // note this shouldn't be used: The previous rotation should be used instead. 
                break;
            case 1:
                rotations = 2;
                break;
            case 2:
                rotations = -1;
                break;
            case 3:
                rotations = 0;
                break;
            case 4:
                rotations = 1;
                break;
        }
        return new Quaternion(0, - (float) Math.Sin(rotations * Math.PI / 8), 0, (float) Math.Cos(rotations * Math.PI / 8));
    }
}
