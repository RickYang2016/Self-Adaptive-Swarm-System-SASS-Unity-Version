using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommunicationFramework : MonoBehaviour
{
    public List<perceptiveMonsterState> perceptionList;
    public List<AgentState> CommunicationList;
    public List<perceptiveMonsterState> MonstersList;

    // Start is called before the first frame update
    void Start()
    {

    }
    
    void Update()
    {   // percept monsters
        perceptionList = gameObject.GetComponentInChildren<MonsterPerception>().agentsInfo;
        // get all connected explorers
        GetUnionList(gameObject.GetComponent<AgentState>());
        // get all percepted monsters
        GetMonstersList(CommunicationList);

    }

    private void GetUnionList(AgentState currentAgent)
    {
        Queue<AgentState> tmpQueue = new Queue<AgentState>();
        CommunicationList = new List<AgentState>();

        tmpQueue.Enqueue(currentAgent);

        while(tmpQueue.Count > 0)
        {
            AgentState agent = tmpQueue.Dequeue();

            foreach(var item in agent.GetComponent<AgentBehaviour>().agentsInfo)
            {                
                if(!CommunicationList.Contains(item))
                {
                    CommunicationList.Add(item);
                    tmpQueue.Enqueue(item);
                }
            }
        }
    }

    private void GetMonstersList(List<AgentState> currentComList)
    {
        MonstersList = new List<perceptiveMonsterState>();

        foreach(var item in currentComList)
        {
            foreach(var agent in item.GetComponent<CommunicationFramework>().perceptionList)
            {
                if(!MonstersList.Contains(agent))
                {
                    MonstersList.Add(agent);
                }
            }
        }
    }
}
