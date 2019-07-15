using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBehaviour : MonoBehaviour
{
    public List<Collider> agents = new List<Collider>();
    private MonsterState agentState;
    public List<MonsterState> agentsInfo;

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
        agentState = agent.GetComponent<MonsterState>();

        // update agent's state and entire agents' list information
        if(agent.tag == "Monster")
        {
            if(!agents.Contains(agent))
            {
                agents.Add(agent);
            }
            if(!agentsInfo.Contains(agentState))
            {
                agentsInfo.Add(agentState);
            } 
        }
    }

    private void OnTriggerExit(Collider agent)
    {
        agentState = agent.GetComponent<MonsterState>();

        if(agent.tag == "Monster")
        {
            agents.Remove(agent);
            agentsInfo.Remove(agentState);
        }
    }

}
