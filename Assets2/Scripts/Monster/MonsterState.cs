using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterState : MonoBehaviour
{
    public string ID;
    public float agentEnergy;
    public float agentHP;
    public Vector3 agentPos;

    // Start is called before the first frame update
    private void Start()
    {
        ID = gameObject.name;
        agentEnergy = 90.0f;
        agentHP = 100.0f;
    }
}
