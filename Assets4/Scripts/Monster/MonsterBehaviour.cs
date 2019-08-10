using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using System;

public class MonsterBehaviour : MonoBehaviour
{
    public float step;
    public List<Collider> agents = new List<Collider>();
    private MonsterState agentState;
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
        step = gameObject.GetComponent<MonsterState>().agentSpeed * Time.deltaTime;
        SortedExplorersDic = new Dictionary<string, Vector3>();
        gameObject.GetComponent<MonsterState>().agentPos = transform.position;

        // whether or not have enough HP
         if(gameObject.GetComponent<MonsterState>().agentHP > 10f)
        {
            // pursue the sepcifical explorer
            if(gameObject.GetComponent<MonsterCom>().perceptionList.Count != 0)
            {
                SortedExplorersDic = GetSortedExplorersList(gameObject.GetComponent<MonsterCom>().perceptionList,
                                                            gameObject.GetComponent<MonsterState>().level2Decision);

                if(Vector3.Distance(SortedExplorersDic.Values.First(), gameObject.GetComponent<MonsterState>().agentPos) > 10)
                {
                    gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition,
                                                                        SortedExplorersDic.Values.First() + offsetDistance, step);
                }
                else
                {
                    gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                }            
            }
        }
        else
        {
            gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition,
                                                                     gameObject.GetComponent<MonsterState>().safetyPos, step);
        }
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
}
