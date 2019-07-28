using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using System;

public class Selection : MonoBehaviour
{
    public string player;
    public int myIndex = 0;
    public int myTask = 0;
    public int numAgent;
    public int numTask;
    // public int agentsTaskIndex = 0;
    public List<string> sortedList;
    public List<int> groupMembersIndexList;
    public Dictionary<string, int> groupList;
    private Dictionary<string, float> energyQueue;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        numAgent = gameObject.GetComponent<CommunicationFramework>().CommunicationList.Count;
        // numTask  = gameObject.GetComponent<CommunicationFramework>().CommunicationList.Count;
        numTask = 3; // numbers of sub-groups

        player  = gameObject.name;
        energyQueue = new Dictionary<string, float>();

        foreach(var item in gameObject.GetComponent<CommunicationFramework>().CommunicationList)
        {
            energyQueue.Add(item.name, item.agentEnergy);
        }

        Dictionary<string, float> energyQueueSort = new Dictionary<string, float>();

        energyQueueSort = Sort(energyQueue);

        sortedList = new List<string>();

        foreach(var item in energyQueueSort)
        {
            sortedList.Add(item.Key);
        }

        myIndex = 0;
        myTask  = 0;

        SelectionPlan(numAgent, player, energyQueueSort, numTask);
    }

    private float MAX(float p1, float p2)
    {
        return p1 > p2 ? p1 : p2;
    }

    // sort the dictionary based on the energy level, first order by the value, if the values are same, then order by key.
    private Dictionary<string, float> Sort(Dictionary<string, float> energyQueue)
    {
        int tmp;
        Dictionary<int, float> tmpQueue = new Dictionary<int, float>();
        Dictionary<string, float> energyQueueSort = new Dictionary<string, float>();

        foreach(var item in energyQueue)
        {
            string tmps = Regex.Replace(item.Key, "[a-z]", "", RegexOptions.IgnoreCase);
            int.TryParse(tmps, out tmp);
            tmpQueue.Add(tmp, item.Value);
        }

        Dictionary<int, float> tmpEnergyQueueSort = tmpQueue.OrderBy(o => o.Value).ThenBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);

        foreach(var item in tmpEnergyQueueSort)
        {
            string temp = Convert.ToString(item.Key);
            temp = temp.Insert(0, "Explorer");
            energyQueueSort.Add(temp, item.Value);
        }

        return energyQueueSort;
    }

    // using linear partition assign tasks
    public void SelectionPlan(int numAgent, string player, Dictionary<string, float> energyQueue, int numTask)
    {
        int n = numAgent;
        int k = numTask;
        // int agentsTaskIndex = 0;
        float [,] m = new float[k, n];
        int [,] d = new int[k, n];
        List<float> p = new List<float>();
        List<int> partitionPlan = new List<int>();
        List<float> energyList = new List<float>();

        foreach(var item in energyQueue)
        {
            energyList.Add(item.Value);
        }

        p = energyList;
        
        for(int i = 1; i < n; i++)
        {
            p[i] = p[i-1] + energyList[i];
        }

        for(int i = 0; i < k; i++)
        {
            m[i, 0] = p[i];
        }

        for(int i = 0; i < n; i++)
        {
            m[0, i] = energyList[i];
        }

        for(int i = 1; i < n; i++)
        {
            for(int j = 1; j < k; j++)
            {
                m[j, i] = 999999f;
                for(int l = 0; l < i; l++)
                {
                    float s = MAX(m[j-1, l], p[i] - p[l]);

                    if(m[j, i] > s)
                    {
                        m[j, i] = s;
                        d[j, i] = l;
                    }
                }
            }
        }

        while(k > 1)
        {
            partitionPlan.Add(d[k-1, n-1]);
            n = d[k-1, n-1] + 1;
            k = k - 1;
        }

        partitionPlan.Reverse();

        foreach(var item in energyQueue)
        {
            if(item.Key == player)
            {
                break;
            }
            myIndex++;
        }

        if(partitionPlan.Count == 0)
        {
            myIndex = myIndex + 1;
            myTask  = myTask  + 1;
        }
        else
        {
            for(int i = 0; i < numTask - 1; i++)
            {
                if(myIndex > partitionPlan[i])
                {
                    myTask = myTask + 1;
                }
            }
            myIndex = myIndex + 1;
            myTask  = myTask  + 1;
        }

        GetGroupMembersIndex(partitionPlan, energyQueue);
    }

    // get the group list index
    public void GetGroupMembersIndex(List<int> partitionPlan, Dictionary<string, float> energyQueue)
    {
        int agentsTaskIndex = 0;

        groupList = new Dictionary<string, int>();
        groupMembersIndexList = new List<int>();

        if(partitionPlan.Count == 0)
        {
            foreach(var item in energyQueue)
            {
                groupMembersIndexList.Add(1);
                groupList.Add(item.Key, 1);
            }
        }
        else
        {
            for(int i = 0; i < partitionPlan.Count + 1; i++)
            {
                int z = 0;

                agentsTaskIndex = agentsTaskIndex + 1;

                if(i < partitionPlan.Count)
                {
                    foreach(var item in energyQueue)
                    {                    
                        z++;

                        if(i == 0)
                        {
                            if(z > partitionPlan[i] + 1)
                            {
                                break;
                            }
                            else
                            {                 
                                groupMembersIndexList.Add(agentsTaskIndex);
                                groupList.Add(item.Key, agentsTaskIndex);
                            }
                        }
                        else
                        {
                            if(z > partitionPlan[i-1] + 1 && z <= partitionPlan[i] + 1)
                            {
                                groupMembersIndexList.Add(agentsTaskIndex);
                                groupList.Add(item.Key, agentsTaskIndex);
                            }
                            else
                            {
                                continue;
                            }
                        }                        
                    }
                    z--;
                }
                else
                {
                    int u = 0;

                    foreach(var item in energyQueue)
                    {
                        u++;

                        if(u > partitionPlan[i-1] + 1)
                        {
                            groupMembersIndexList.Add(agentsTaskIndex);               
                            groupList.Add(item.Key, agentsTaskIndex);
                        }
                    }
                }
                
            }
        }
    }
}
