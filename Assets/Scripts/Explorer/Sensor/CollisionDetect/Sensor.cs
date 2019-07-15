using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public List<Collider> agents = new List<Collider>();
    private AgentState agentState;
    public List<AgentState> agentsInfo;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider agent)
    {
        agentState = agent.GetComponentInParent<AgentState>();

        // update agent's state and entire agents' list information
        if(agent.tag == "Collision")
        {
            agents.Add(agent);
            agentsInfo.Add(agentState);
        }
    }

    private void OnTriggerExit(Collider agent)
    {
        agentState = agent.GetComponent<AgentState>();

        if(agent.tag == "Collision")
        {
            agents.Remove(agent);
            agentsInfo.Remove(agentState);
        }
    }
}