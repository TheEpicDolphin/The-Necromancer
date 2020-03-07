using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = target.position - 100.0f * target.forward;
        transform.position = Vector3.Lerp(transform.position, newPos, 5.0f * Time.deltaTime);
    }
}
