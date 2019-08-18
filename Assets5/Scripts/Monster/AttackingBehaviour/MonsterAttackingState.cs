using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackingState : MonoBehaviour
{
    public float currentHP = 100f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider agent)
    {
        if(agent.tag == "ExplorerAttackingRange")
        {
            currentHP = currentHP - 0.05f;
        }
    }
}
