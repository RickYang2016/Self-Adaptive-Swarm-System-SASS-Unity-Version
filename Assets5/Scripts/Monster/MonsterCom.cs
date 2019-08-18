using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterCom : MonoBehaviour
{
    public List<perceptiveExplorerState> perceptionList;
    public List<MonsterState> CommunicationList;
    public List<perceptiveExplorerState> ExplorersList;

    // Start is called before the first frame update
    void Start()
    {

    }
    
    void Update()
    {
        // percept the explorers
        perceptionList = gameObject.GetComponentInChildren<ExplorerPerception>().agentsInfo;
        // get all connected monsters
        GetUnionList(gameObject.GetComponent<MonsterState>());
        // get all percepted explorers
        GetExplorersList(CommunicationList);
    }

    private void GetUnionList(MonsterState currentAgent)
    {
        Queue<MonsterState> tmpQueue = new Queue<MonsterState>();
        CommunicationList = new List<MonsterState>();

        tmpQueue.Enqueue(currentAgent);

        while(tmpQueue.Count > 0)
        {
            MonsterState agent = tmpQueue.Dequeue();

            foreach(var item in agent.GetComponent<MonsterBehaviour>().agentsInfo)
            {
                
                if(!CommunicationList.Contains(item))
                {
                    CommunicationList.Add(item);
                    tmpQueue.Enqueue(item);
                }
            }
        }
    }

    private void GetExplorersList(List<MonsterState> currentComList)
    {
        ExplorersList = new List<perceptiveExplorerState>();

        foreach(var item in currentComList)
        {
            foreach(var agent in item.GetComponent<MonsterCom>().perceptionList)
            {
                if(!ExplorersList.Contains(agent))
                {
                    ExplorersList.Add(agent);
                }
            }
        }
    }
}
