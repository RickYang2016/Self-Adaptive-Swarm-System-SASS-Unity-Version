using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSensor : MonoBehaviour
{
    public List<Collider> agents = new List<Collider>();
    private MonsterSensorState agentState;
    public List<MonsterSensorState> agentsInfo;
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
        agentState = agent.GetComponentInParent<MonsterSensorState>();

        // update agent's state and entire agents' list information
        if(agent.tag == "MonsterCollision")
        {
            agents.Add(agent);
            agentsInfo.Add(agentState);
        }
    }

    private void OnTriggerExit(Collider agent)
    {
        agentState = agent.GetComponent<MonsterSensorState>();

        if(agent.tag == "MonsterCollision")
        {
            agents.Remove(agent);
            agentsInfo.Remove(agentState);
        }
    }
}
