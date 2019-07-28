using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AgentState : MonoBehaviour
{
    public string ID;
    public float agentEnergy;
    public float agentHP;
    public float agentRadius;
    public float agentSpeed;
    public Vector3 agentPos;
    public Vector3 goalPos;
    public string level1Decision;
    public string level2Decision;
    public int level3Decision;
    public bool isFinished;

    // Start is called before the first frame update
    void Start()
    {
        ID = gameObject.name;
        agentEnergy = 90.0f;
        agentHP = 100.0f;
        agentRadius = 3f;
        agentSpeed = 10.0f;
        goalPos = new Vector3(0f, 0.25f, 0f);
        level1Decision = "Defending";
        isFinished = false;
    }
}
