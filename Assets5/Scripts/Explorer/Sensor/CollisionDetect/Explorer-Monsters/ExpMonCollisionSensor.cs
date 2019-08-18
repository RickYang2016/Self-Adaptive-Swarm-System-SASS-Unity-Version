using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpMonCollisionSensor : MonoBehaviour
{
    public List<Collider> agents = new List<Collider>();
    public List<MonExpColSensorState> agentsInfo;
    private MonExpColSensorState agentState;

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
        agentState = agent.GetComponent<MonExpColSensorState>();

        // update agent's state and entire agents' list information
        if(agent.tag == "ExpMonCollision")
        {
            agents.Add(agent);
            agentsInfo.Add(agentState);
        }
    }

    private void OnTriggerExit(Collider agent)
    {
        agentState = agent.GetComponent<MonExpColSensorState>();

        if(agent.tag == "ExpMonCollision")
        {
            agents.Remove(agent);
            agentsInfo.Remove(agentState);
        }
    }
}
