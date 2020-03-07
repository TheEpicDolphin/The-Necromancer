using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    public float radius;
    public Vector3 velocity;
    float playerSpeed = 15.0f;
    Magic equipedMagic;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 spriteBounds = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().bounds.size;
        radius = spriteBounds.x / 2;

        equipedMagic = gameObject.AddComponent<Fire>();
        transform.forward = -Camera.main.transform.forward;
        transform.up = Camera.main.transform.up;
    }

    // Update is called once per frame
    void Update()
    {
        velocity = Vector3.zero;
        if (Input.GetKey(KeyCode.A))
        {
            velocity += playerSpeed * new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.W))
        {
            velocity += playerSpeed * new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.D))
        {
            velocity += playerSpeed * new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            velocity += playerSpeed * new Vector3(0, 0, -1);
        }

        
        transform.position += Mathf.Clamp(velocity.magnitude, 0.0f, playerSpeed) * velocity.normalized * Time.deltaTime;

        if (Input.GetMouseButton(0))
        {
            equipedMagic.Cast();
        }
    }

}

