using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using System;

public class MonsterBehaviour : MonoBehaviour
{
    public float normalStep;
    public float emergencyStep;
    public List<Collider> agents = new List<Collider>();
    private MonsterState agentState;
    private int countTime = 0;
    public List<MonsterState> agentsInfo;
    public Dictionary<string, Vector3> SortedExplorersDic;
    private Vector3 offsetDistance = new Vector3(20f, 0f, 10f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // initialize target point
        normalStep = gameObject.GetComponent<MonsterState>().agentSpeed * Time.deltaTime;
        emergencyStep = gameObject.GetComponent<MonsterState>().agentSpeed * 3 * Time.deltaTime;
        SortedExplorersDic = new Dictionary<string, Vector3>();
        gameObject.GetComponent<MonsterState>().agentPos = transform.position;

        // whether or not have enough HP
         if(gameObject.GetComponent<MonsterState>().agentHP > 10f)
        {
            if(gameObject.GetComponent<MonsterCom>().perceptionList.Count == 0)
            {
                gameObject.GetComponent<MonsterState>().level1Decision = "Patroling";

                Patroling();
            }
            // pursue the sepcifical explorer
            else if(gameObject.GetComponent<MonsterCom>().perceptionList.Count != 0 && gameObject.GetComponent<MonsterState>().level1Decision == "Attacking")
            {
                if(gameObject.GetComponent<MonsterState>().level3Decision == "Independent")
                {
                    Attacking();
                }
                else if(gameObject.GetComponent<MonsterState>().level3Decision == "Dependent")
                {
                    if(gameObject.GetComponent<MonsterCom>().CommunicationList.Count != 0)
                    {
                        FollowTheNearestMonster();
                    }
                    else
                    {
                        Attacking();
                    }
                }
                else
                {
                    Debug.Log("When monster attack, have some problem in Monster level3 decision.");
                }
            }
            else if(gameObject.GetComponent<MonsterCom>().perceptionList.Count != 0 && gameObject.GetComponent<MonsterState>().level1Decision == "Defending")
            {
                if(gameObject.GetComponent<MonsterState>().level3Decision == "Independent")
                {
                    Defending();
                }
                else if(gameObject.GetComponent<MonsterState>().level3Decision == "Dependent")
                {
                    if(gameObject.GetComponent<MonsterCom>().CommunicationList.Count != 0)
                    {
                        FollowTheNearestMonster();
                    }
                    else
                    {
                        Defending();
                    }
                }
                else
                {
                    Debug.Log("When monster defend, have some problem in Monster level3 decision.");
                }             
            }
        }
        else
        {
            // if(gameObject.GetComponentInChildren<MonExpCollisionSensor>().agentsInfo.Count != 0)
            // {
            //     gameObject.GetComponent<MonsterRouting>().MonExpCollisionRoutingPlan(gameObject.GetComponentInChildren<MonExpCollisionSensor>().agentsInfo);
            // }
            // else
            // {
                gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition,
                                                                     gameObject.GetComponent<MonsterState>().safetyPos, emergencyStep);
            // }
        }
    }

    private void OnTriggerEnter(Collider agent)
    {
        agentState = agent.GetComponent<MonsterState>();

        // update agent's state and entire agents' list          
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

    // get sorted explorers' list based on specifical law
    private Dictionary<string, Vector3> GetSortedExplorersList(List<perceptiveExplorerState> ExplorersList, string level2Decision)
    {
        Dictionary<string, Vector3> sortedExplorersList = new Dictionary<string, Vector3>();
        Dictionary<string, float> tmpsortedExplorersList = new Dictionary<string, float>();
        Dictionary<string, float> inputList = new Dictionary<string, float>();

        if(level2Decision == "Nearest")
        {
            foreach(var item1 in  ExplorersList)
            {
                inputList.Add(item1.name, Vector3.Distance(item1.agentPos, gameObject.GetComponent<MonsterState>().agentPos));
            }

            tmpsortedExplorersList = Sort(inputList, level2Decision);
        }
        else if(level2Decision == "Lowest_Attacking_Ability")
        {
            foreach(var item in ExplorersList)
            {
                inputList.Add(item.name, item.agentEnergy);
            }

            tmpsortedExplorersList = Sort(inputList, level2Decision);
        }
        else if(level2Decision == "Highest_Attacking_Ability")
        {
            foreach(var item in ExplorersList)
            {
                inputList.Add(item.name, item.agentEnergy);
            }

            tmpsortedExplorersList = Sort(inputList, level2Decision);
        }
        else
        {
            Debug.Log("Monster's level2 decision has something problem!");
        }

        foreach(var item1 in tmpsortedExplorersList)
        {
            foreach(var item2 in ExplorersList)
            {
                if(item1.Key == item2.ID)
                {
                    sortedExplorersList.Add(item1.Key, item2.agentPos);
                }
            }
        }

        return sortedExplorersList;
    }

    // sort the dictionary based on the level2 decision.
    private Dictionary<string, float> Sort(Dictionary<string, float> inputList, string level2Decision)
    {
        int tmp;
        Dictionary<int, float> tmpList = new Dictionary<int, float>();
        Dictionary<string, float> sortedList = new Dictionary<string, float>();
        Dictionary<int, float> tmpListSort = new Dictionary<int, float>();

        foreach(var item in inputList)
        {
            string tmps = Regex.Replace(item.Key, "[a-z]", "", RegexOptions.IgnoreCase);
            int.TryParse(tmps, out tmp);
            tmpList.Add(tmp, item.Value);
        }

        if(level2Decision == "Highest_Attacking_Ability")
        {
            tmpListSort = tmpList.OrderByDescending(o => o.Value).ThenBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
        }
        else
        {
            tmpListSort = tmpList.OrderBy(o => o.Value).ThenBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
        }

        foreach(var item in tmpListSort)
        {
            string temp = Convert.ToString(item.Key);
            temp = temp.Insert(0, "Explorer");
            sortedList.Add(temp, item.Value);
        }

        return sortedList;
    }

    private void Patroling()
    {
        float tmp = 0;
        string tmps = Regex.Replace(gameObject.GetComponent<MonsterState>().ID, "[a-z]", "", RegexOptions.IgnoreCase);
        float.TryParse(tmps, out tmp);

        countTime++;

        if (countTime < 200)
       {

            transform.position = new Vector3(transform.position.x + tmp * Time.deltaTime, transform.position.y, transform.position.z);
        }

        if (countTime > 200 && countTime < 400)
        {
           transform.position = new Vector3(transform.position.x - tmp * Time.deltaTime, transform.position.y, transform.position.z);
        }

        if (countTime > 400)
        {
            countTime = 0;
        }

    }

    private void Attacking()
    {
        SortedExplorersDic = GetSortedExplorersList(gameObject.GetComponent<MonsterCom>().perceptionList,
                                                    gameObject.GetComponent<MonsterState>().level2Decision);

        if(Vector3.Distance(SortedExplorersDic.Values.First(), gameObject.GetComponent<MonsterState>().agentPos) > 10)
        {
            gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition,
                                                                SortedExplorersDic.Values.First() + offsetDistance, normalStep);
        }
        else
        {
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    private void Defending()
    {
        SortedExplorersDic = GetSortedExplorersList(gameObject.GetComponent<MonsterCom>().perceptionList,
                                                    gameObject.GetComponent<MonsterState>().level2Decision);

        Vector3 moveDirection = new Vector3();
        float tmpDis = Vector3.Distance(SortedExplorersDic.Values.First(), gameObject.GetComponent<MonsterState>().agentPos);

        moveDirection = gameObject.transform.localPosition - SortedExplorersDic.Values.First();

        if(tmpDis > 10 && tmpDis < 20)
        {
            gameObject.GetComponent<Rigidbody>().AddForce(moveDirection * 0.1f, ForceMode.Acceleration);
        }
        else
        {
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    // sort the dictionary based on the distance, first order by the value, if the values are same, then order by key.
    private Dictionary<string, float> Sort(Dictionary<string, float> infoDic)
    {
        int tmp;
        Dictionary<int, float> tmpDic = new Dictionary<int, float>();
        Dictionary<string, float> infoDicSort = new Dictionary<string, float>();

        foreach(var item in infoDic)
        {
            string tmps = Regex.Replace(item.Key, "[a-z]", "", RegexOptions.IgnoreCase);
            int.TryParse(tmps, out tmp);
            tmpDic.Add(tmp, item.Value);
        }

        Dictionary<int, float> tmpinfoDicSort = tmpDic.OrderBy(o => o.Value).ThenBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);

        foreach(var item in tmpinfoDicSort)
        {
            string temp = Convert.ToString(item.Key);
            temp = temp.Insert(0, "Monster");
            infoDicSort.Add(temp, item.Value);
        }

        return infoDicSort;
    }
    private void FollowTheNearestMonster()
    {
        string nearestMonsterID;
        Vector3 nearestMonsterPOS = new Vector3();
        Dictionary<string, float> disMatrix = new Dictionary<string, float>();

        foreach(var item in gameObject.GetComponent<MonsterCom>().CommunicationList)
        {
            disMatrix.Add(item.ID, Vector3.Distance(gameObject.GetComponent<MonsterState>().agentPos, item.agentPos));
        }

        nearestMonsterID = Sort(disMatrix).Keys.First();

        foreach(var item in gameObject.GetComponent<MonsterCom>().CommunicationList)
        {
            if(nearestMonsterID == item.ID)
            {
                nearestMonsterPOS = item.agentPos;
            }
        }        

        gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition,
                                                                nearestMonsterPOS + offsetDistance, normalStep);
    }
}
