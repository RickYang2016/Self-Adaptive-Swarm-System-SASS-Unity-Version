using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPerception : MonoBehaviour
{
    public List<Collider> agents = new List<Collider>();
    private MonsterState agentState;
    public List<MonsterState> agentsInfo;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider agent)
    {
        agentState = agent.GetComponent<MonsterState>();

        // update agent's state and entire agents' list information
        if(agent.tag == "MonsterPerception")
        {
            agents.Add(agent);
            agentsInfo.Add(agentState);
        }
    }

    private void OnTriggerExit(Collider agent)
    {
        agentState = agent.GetComponent<MonsterState>();

        if(agent.tag == "MonsterPerception")
        {
            agents.Remove(agent);
            agentsInfo.Remove(agentState);
        }
    }
}
