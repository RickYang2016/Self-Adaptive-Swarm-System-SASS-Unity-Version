using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonExpCollisionSensor : MonoBehaviour
{
    public List<Collider> agents = new List<Collider>();
    public List<ExpMonColSensorState> agentsInfo;
    private ExpMonColSensorState agentState;

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
        agentState = agent.GetComponent<ExpMonColSensorState>();

        // update agent's state and entire agents' list information
        if(agent.tag == "MonExpCollision")
        {
            agents.Add(agent);
            agentsInfo.Add(agentState);
        }
    }

    private void OnTriggerExit(Collider agent)
    {
        agentState = agent.GetComponent<ExpMonColSensorState>();

        if(agent.tag == "MonExpCollision")
        {
            agents.Remove(agent);
            agentsInfo.Remove(agentState);
        }
    }
}
