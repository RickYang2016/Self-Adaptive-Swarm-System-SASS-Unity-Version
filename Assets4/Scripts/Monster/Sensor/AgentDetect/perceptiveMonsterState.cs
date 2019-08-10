using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class perceptiveMonsterState : MonoBehaviour
{
    public string ID;
    public float agentEnergy;
    public float agentHP;
    public float agentRadius;
    public float agentSpeed;
    public float unitAttackingEnergyCost;
    public Vector3 agentPos;

    // Start is called before the first frame update
    void Start()
    {
        ID = gameObject.GetComponentInParent<MonsterState>().name;
        agentRadius = 5f;
        agentSpeed = 15.0f;
        unitAttackingEnergyCost = 0.3f;
    }

    // Update is called once per frame
    void Update()
    {
        agentEnergy = gameObject.GetComponentInParent<MonsterState>().agentEnergy;
        agentPos = gameObject.GetComponentInParent<MonsterState>().agentPos;
        agentHP = gameObject.GetComponentInParent<MonsterState>().agentHP;
    }
}
