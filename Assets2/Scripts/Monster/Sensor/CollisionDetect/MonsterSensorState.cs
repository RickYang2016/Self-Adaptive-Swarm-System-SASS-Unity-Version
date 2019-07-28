using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSensorState : MonoBehaviour
{
    public string ID;
    public float detectRadius;
    public float agentEnergy;
    // Start is called before the first frame update
    void Start()
    {
        ID = gameObject.GetComponentInParent<MonsterState>().name;
        detectRadius = gameObject.GetComponent<SphereCollider>().radius;
        agentEnergy = gameObject.GetComponentInParent<MonsterState>().agentEnergy;
    }
}
