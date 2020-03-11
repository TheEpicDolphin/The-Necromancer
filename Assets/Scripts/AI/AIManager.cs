using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    private static AIManager _instance;
    public static AIManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public List<Zombie> zombies = new List<Zombie>();
    public PlayerMovementController playerAgent;

    
    // Start is called before the first frame update
    void Start()
    {
        playerAgent = transform.parent.gameObject.GetComponent<PlayerMovementController>();
    }

    // Update is called once per frame
    void Update()
    {

    }


}
