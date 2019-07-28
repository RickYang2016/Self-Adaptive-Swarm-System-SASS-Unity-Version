using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class perceptiveMonsterState : MonoBehaviour
{
    public string ID;
    public float agentEnergy;
    public Vector3 agentPos;

    // Start is called before the first frame update
    void Start()
    {
        ID = gameObject.GetComponentInParent<MonsterState>().name;
    }
}
