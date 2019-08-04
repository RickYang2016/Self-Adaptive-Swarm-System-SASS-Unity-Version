using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorerAttackingState : MonoBehaviour
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
        if(agent.tag == "MonsterAttackingRange")
        {
            currentHP = currentHP - 0.1f;
        }
    }
}
