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
    public Vector3 agentPos;
    public float unitAttackingEnergyCost;

    // Start is called before the first frame update
    void Start()
    {
        ID = gameObject.GetComponentInParent<MonsterState>().name;
    }

    // Update is called once per frame
    void Update()
    {
        agentEnergy = gameObject.GetComponentInParent<MonsterState>().agentEnergy;
        agentPos = gameObject.GetComponentInParent<MonsterState>().agentPos;
        agentHP = gameObject.GetComponentInParent<MonsterState>().agentHP;
        agentRadius = gameObject.GetComponentInParent<MonsterState>().agentRadius;
        agentSpeed = gameObject.GetComponentInParent<MonsterState>().agentSpeed;
        unitAttackingEnergyCost = gameObject.GetComponentInParent<MonsterState>().unitAttackingEnergyCost;
    }
}
