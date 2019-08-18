using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonExpColSensorState : MonoBehaviour
{
    public string ID;
    public float detectRadius;
    public float agentRadius;
    public Vector3 agentPos;

    // Start is called before the first frame update
    void Start()
    {
        ID = gameObject.GetComponentInParent<MonsterState>().name;
        detectRadius = gameObject.GetComponent<SphereCollider>().radius;
        
    }

    // Update is called once per frame
    void Update()
    {
        agentPos = gameObject.GetComponentInParent<MonsterState>().agentPos;
        agentRadius = gameObject.GetComponentInParent<MonsterState>().agentRadius;
    }
}
