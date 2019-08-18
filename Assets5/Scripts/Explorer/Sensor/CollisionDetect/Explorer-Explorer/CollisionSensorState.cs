using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSensorState : MonoBehaviour
{
    public string ID;
    public float detectRadius;
    public float agentEnergy;
    public Vector3 agentPos;
    // Start is called before the first frame update
    void Start()
    {
        ID = gameObject.GetComponentInParent<AgentState>().name;
        detectRadius = gameObject.GetComponent<SphereCollider>().radius;        
    }

    // Update is called once per frame
    void Update()
    {
        agentEnergy = gameObject.GetComponentInParent<AgentState>().agentEnergy;
        agentPos = gameObject.GetComponentInParent<AgentState>().agentPos;
    }
}
