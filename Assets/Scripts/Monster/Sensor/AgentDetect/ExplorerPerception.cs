using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorerPerception : MonoBehaviour
{
    public List<Collider> agents = new List<Collider>();
    private AgentState agentState;
    public List<AgentState> agentsInfo;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider agent)
    {
        agentState = agent.GetComponent<AgentState>();

        // update agent's state and entire agents' list information
        if(agent.tag == "ExplorerPerception")
        {
            agents.Add(agent);
            agentsInfo.Add(agentState);
        }
    }

    private void OnTriggerExit(Collider agent)
    {
        agentState = agent.GetComponent<AgentState>();

        if(agent.tag == "ExplorerPerception")
        {
            agents.Remove(agent);
            agentsInfo.Remove(agentState);
        }
    }
}
