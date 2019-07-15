using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDection : MonoBehaviour
{
    public List<AgentState> CollisionList;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetUnionList(gameObject.GetComponent<AgentState>());       
    }

    private void GetUnionList(AgentState currentAgent)
    {
        Queue<AgentState> tmpQueue = new Queue<AgentState>();
        CollisionList = new List<AgentState>();

        tmpQueue.Enqueue(currentAgent);

        while(tmpQueue.Count > 0)
        {
            AgentState agent = tmpQueue.Dequeue();

            foreach(var item in agent.GetComponent<Sensor>().agentsInfo)
            {
                
                if(!CollisionList.Contains(item))
                {
                    CollisionList.Add(item);
                    tmpQueue.Enqueue(item);
                }
            }
        }
    }
}
