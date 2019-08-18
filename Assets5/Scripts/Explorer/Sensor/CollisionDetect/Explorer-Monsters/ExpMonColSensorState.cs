using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpMonColSensorState : MonoBehaviour
{
    public string ID;
    public float detectRadius;
    public float agentRadius;
    public Vector3 agentPos;
    
    // Start is called before the first frame update
    void Start()
    {
        ID = gameObject.GetComponentInParent<AgentState>().name;
        detectRadius = gameObject.GetComponent<SphereCollider>().radius;
        agentRadius = gameObject.GetComponentInParent<AgentState>().agentRadius;    
    }

    // Update is called once per frame
    void Update()
    {
        agentPos = gameObject.GetComponentInParent<AgentState>().agentPos;
    }
}
