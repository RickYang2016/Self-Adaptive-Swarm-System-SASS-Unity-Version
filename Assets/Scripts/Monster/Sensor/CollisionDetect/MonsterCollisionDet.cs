using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterCollisionDet : MonoBehaviour
{
    public List<MonsterState> CollisionList;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetUnionList(gameObject.GetComponent<MonsterState>());       
    }

    private void GetUnionList(MonsterState currentAgent)
    {
        Queue<MonsterState> tmpQueue = new Queue<MonsterState>();
        CollisionList = new List<MonsterState>();

        tmpQueue.Enqueue(currentAgent);

        while(tmpQueue.Count > 0)
        {
            MonsterState agent = tmpQueue.Dequeue();

            foreach(var item in agent.GetComponent<MonsterSensor>().agentsInfo)
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
