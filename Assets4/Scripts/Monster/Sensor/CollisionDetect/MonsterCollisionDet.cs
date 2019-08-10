using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterCollisionDet : MonoBehaviour
{
    public List<MonsterSensorState> CollisionList;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetUnionList(gameObject.GetComponent<MonsterSensorState>());       
    }

    private void GetUnionList(MonsterSensorState currentAgent)
    {
        Queue<MonsterSensorState> tmpQueue = new Queue<MonsterSensorState>();
        CollisionList = new List<MonsterSensorState>();

        tmpQueue.Enqueue(currentAgent);

        while(tmpQueue.Count > 0)
        {
            MonsterSensorState agent = tmpQueue.Dequeue();

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
