using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class perceptiveExplorerState : MonoBehaviour
{
    public string ID;
    public float agentEnergy;
    public float agentHP;
    public float agentRadius;
    public float agentSpeed;
    public Vector3 agentPos;
    public float unitAttackingEnergyCost;

    // Start is called before the first frame update
    void Start()
    {
        ID = gameObject.GetComponentInParent<AgentState>().name;
    }

    // Update is called once per frame
    void Update()
    {
        agentEnergy = gameObject.GetComponentInParent<AgentState>().agentEnergy;
        agentPos = gameObject.GetComponentInParent<AgentState>().agentPos;
        agentHP = gameObject.GetComponentInParent<AgentState>().agentHP;
        agentRadius = gameObject.GetComponentInParent<AgentState>().agentRadius;
        agentSpeed = gameObject.GetComponentInParent<AgentState>().agentSpeed;
        unitAttackingEnergyCost = gameObject.GetComponentInParent<AgentState>().unitAttackingEnergyCost;
    }
}
