using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using System;

public class Formation : MonoBehaviour
{
    public string player;
    public int groupNum; // belong to which group
    public int numAgents;
    public Vector3 targetMonsterPos; // target monster's position
    public Vector3 agentFinalPos; // current agent's final formation position
    public List<Vector3> tasksList; // all the tasks' coordinate
    public List<float> finalDisList;
    public List<Vector3> finalPosList; // the position of all the robots in the same group
    public float groupDeflectionAngle = 0;
    private Dictionary<string, Vector3> robotsFinalPos;
    private float robotRadius;
    private List<float> distVector;       
    private List<Vector3> robotsPos;
    private float robotDis = 0; // the distance between robot's current position and destination point
    private float theta = 0;
    private float currentRadius = 0;
    private Vector3 robotPos = new Vector3();
    private Vector3 taskCoordinate = new Vector3();
    private Dictionary<string, List<float>> robotsTaskDis;
    private Dictionary<string, List<Vector3>> robotsTaskPos;
    private Dictionary<string, Vector3> groupMemberPos; // robots are in the same group sorted by energy level
    private Dictionary<string, Vector3> SortedMonstersDic;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(gameObject.GetComponent<Selection>().groupList != null)
        {
            FormationPlan();
        }
    }

    public void FormationPlan()
    {
        Vector3 offsetDistance = new Vector3(-20f, 0f, 10f);

        numAgents = 0;
        groupMemberPos = new Dictionary<string, Vector3>();        
        player = gameObject.name;
        groupNum = gameObject.GetComponent<Selection>().myTask;
        SortedMonstersDic = new Dictionary<string, Vector3>();
        targetMonsterPos = new Vector3();
        tasksList = new List<Vector3>();

        if(gameObject.GetComponent<AgentState>().level1Decision == "Patroling")
        {
            numAgents = gameObject.GetComponent<CommunicationFramework>().CommunicationList.Count;

            foreach(var item in gameObject.GetComponent<CommunicationFramework>().CommunicationList)
            {
                groupMemberPos.Add(item.name, item.agentPos);
            }

            // tasksList.Add(gameObject.GetComponent<AgentState>().safetyPos);
        }
        else
        {
            foreach(var item1 in gameObject.GetComponent<Selection>().groupList)
            {
                if(item1.Value == groupNum)
                {
                    numAgents++;

                    foreach(var item2 in gameObject.GetComponent<CommunicationFramework>().CommunicationList)
                    {
                        if(item2.ID == item1.Key)
                        {
                            groupMemberPos.Add(item1.Key, item2.agentPos);
                        }
                    }
                }
            }

            // add monsters' information
            if(gameObject.GetComponent<CommunicationFramework>().MonstersList.Count != 0)
            {
                SortedMonstersDic = GetSortedMonstersList(gameObject.GetComponent<CommunicationFramework>().MonstersList,
                                                          gameObject.GetComponent<AgentState>().level2Decision);

                targetMonsterPos = SortedMonstersDic.Values.First() + offsetDistance;
            } 
        }

        // patroling formation
        if(gameObject.GetComponent<AgentState>().level1Decision == "Patroling")
        {
            PatrolFormation(numAgents, player, groupMemberPos, groupNum);
        }

        // attacking formation
        else if(gameObject.GetComponent<AgentState>().level1Decision == "Attacking")
        {
            AttackFormation(numAgents, player, groupMemberPos, groupNum);
        }

        // defending formation
        else if(gameObject.GetComponent<AgentState>().level1Decision == "Defending")
        {
            DefendFormation(numAgents, player, groupMemberPos, groupNum);               
        }
        else
        {
            Debug.Log("Level1 decision has some problem!");
        }
    }

    public Vector3 GetCentralPoint()
    {
        Vector3 centralPoint = new Vector3();
        Vector3 tmpPos = new Vector3(0, 0, 0);

        foreach(var item in gameObject.GetComponent<CommunicationFramework>().CommunicationList)
        {
            tmpPos = tmpPos + item.agentPos;
        }

        centralPoint = tmpPos / gameObject.GetComponent<CommunicationFramework>().CommunicationList.Count;

        return centralPoint;
    }

    public void PatrolFormation(int numAgents, string player, 
                                Dictionary<string, Vector3> groupMemberPos,
                                int groupNum)
    {
        int n = numAgents;

        robotsTaskDis = new Dictionary<string, List<float>>();
        robotsTaskPos = new Dictionary<string, List<Vector3>>();        
        finalPosList = new List<Vector3>();
        robotRadius = gameObject.GetComponent<AgentState>().agentRadius;

        foreach(var robotLocalPos in groupMemberPos)
        {
            int countTimes = 0;

            distVector = new List<float>();
            robotsPos = new List<Vector3>();

            for(int i = 0; i < Math.Ceiling(n/3.0) + 1; i++)
            {
                if(gameObject.GetComponent<Routing>().countTimes < 160)
                {
                    taskCoordinate = GetCentralPoint();
                }
                else
                {
                    taskCoordinate = gameObject.GetComponent<AgentState>().centralGoalPos;
                }

                if(i == 0)
                {
                    countTimes++;
                    robotPos = taskCoordinate;
                    robotDis = Vector3.Distance(robotPos, robotLocalPos.Value);
                    distVector.Add(robotDis);
                    robotsPos.Add(robotPos);
                }
                else
                {
                    foreach(int j in Enumerable.Range(1, 3))
                    {
                        if(countTimes < groupMemberPos.Count)
                        {
                            countTimes++;
                            theta = Convert.ToSingle(j * (2 * Math.PI) / 3);
                            currentRadius = 3 * i * robotRadius;
                            robotPos = taskCoordinate + new Vector3(Convert.ToSingle(currentRadius * Math.Cos(theta)), 0,
                                                                    Convert.ToSingle(currentRadius * Math.Sin(theta)));
                            robotDis = Vector3.Distance(robotPos, robotLocalPos.Value);
                            distVector.Add(robotDis);
                            robotsPos.Add(robotPos);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            robotsTaskDis.Add(robotLocalPos.Key, distVector);
            robotsTaskPos.Add(robotLocalPos.Key, robotsPos);           
        }

        GetFinalDisPos(robotsTaskDis, robotsTaskPos, taskCoordinate, groupNum);
    }

    public void AttackFormation(int attackNumAgent, string player, 
                                Dictionary<string, Vector3> groupMemberPos,
                                int groupNum)
    {
        int n = attackNumAgent;

        robotsTaskDis = new Dictionary<string, List<float>>();
        robotsTaskPos = new Dictionary<string, List<Vector3>>();        
        finalPosList = new List<Vector3>();
        robotRadius = gameObject.GetComponent<AgentState>().agentRadius;

        foreach(var robotLocalPos in groupMemberPos)
        {
            int countTimes = 0;

            distVector = new List<float>();
            robotsPos = new List<Vector3>();

            for(int i = 0; i < Math.Ceiling((Math.Pow((n * 8 + 1), 0.5) - 1) / 2); i++)
            {
                taskCoordinate = targetMonsterPos;

                foreach(int j in Enumerable.Range(1, i+1))
                {
                    if(countTimes < groupMemberPos.Count)
                    {
                        countTimes++;
                        currentRadius = 3 * robotRadius;
                        robotPos = taskCoordinate + new Vector3(Convert.ToSingle(j * currentRadius - i * currentRadius * 1/2), 0,
                                                                Convert.ToSingle(i * currentRadius * Math.Sin(2 * Math.PI/3)));
                        robotDis = Vector3.Distance(robotPos, robotLocalPos.Value);
                        distVector.Add(robotDis);
                        robotsPos.Add(robotPos);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            robotsTaskDis.Add(robotLocalPos.Key, distVector);
            robotsTaskPos.Add(robotLocalPos.Key, robotsPos);         
        }

        GetFinalDisPos(robotsTaskDis, robotsTaskPos, targetMonsterPos, groupNum);
    }

    public void DefendFormation(int defendNumAgent, string player, 
                                Dictionary<string, Vector3> groupMemberPos,
                                int groupNum)
    {
        int n = defendNumAgent;

        robotsTaskDis = new Dictionary<string, List<float>>();
        robotsTaskPos = new Dictionary<string, List<Vector3>>();        
        finalPosList = new List<Vector3>();
        robotRadius = gameObject.GetComponent<AgentState>().agentRadius;

        foreach(var robotLocalPos in groupMemberPos)
        {
            int countTimes = 0;

            distVector = new List<float>();
            robotsPos = new List<Vector3>();

            for(int i = 0; i < Math.Ceiling(n/2.0); i++)
            {
                taskCoordinate = targetMonsterPos;

                foreach(int j in Enumerable.Range(0, 2))
                {
                    currentRadius = 5 * robotRadius;
                
                    if(countTimes < groupMemberPos.Count)
                    {
                        countTimes++;

                        if(i % 2 == 0)
                        {
                            robotPos = taskCoordinate + new Vector3(Convert.ToSingle(j * currentRadius), 0,
                                                                    Convert.ToSingle(i * currentRadius * Math.Sin(5 * Math.PI/6))); 
                            robotDis = Vector3.Distance(robotPos, robotLocalPos.Value);
                            distVector.Add(robotDis);
                            robotsPos.Add(robotPos);
                        }
                        else
                        {
                            robotPos = taskCoordinate + new Vector3(Convert.ToSingle(currentRadius * Math.Cos(7 * Math.PI/6) + 
                                                                    j * currentRadius * (1 + 2 * Math.Cos(Math.PI/6))), 0,
                                                                    Convert.ToSingle(i * currentRadius * Math.Sin(5 * Math.PI/6)));
                            robotDis = Vector3.Distance(robotPos, robotLocalPos.Value);
                            distVector.Add(robotDis);
                            robotsPos.Add(robotPos);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            robotsTaskDis.Add(robotLocalPos.Key, distVector);
            robotsTaskPos.Add(robotLocalPos.Key, robotsPos);            
        }

        GetFinalDisPos(robotsTaskDis, robotsTaskPos, targetMonsterPos, groupNum);
    }

    // get sorted monsters' list based on specifical law
    private Dictionary<string, Vector3> GetSortedMonstersList(List<perceptiveMonsterState> MonstersList, string level2Decision)
    {
        Dictionary<string, Vector3> sortedMonstersList = new Dictionary<string, Vector3>();
        Dictionary<string, float> tmpsortedMonstersList = new Dictionary<string, float>();
        Dictionary<string, float> inputList = new Dictionary<string, float>();
        // Dictionary<string, float> disMatrix;
        List<float> disMatrix;

        if(level2Decision == "Nearest")
        {
            foreach(var item1 in MonstersList)
            {
                disMatrix = new List<float>();

                foreach(var item2 in gameObject.GetComponent<CommunicationFramework>().CommunicationList)
                {
                    // if(Vector3.Distance(item1.agentPos, item2.agentPos) != 0)
                    {
                        disMatrix.Add(Vector3.Distance(item1.agentPos, item2.agentPos));
                    }
                }
                inputList.Add(item1.ID, disMatrix.Min());
                // inputList.Add(disMatrix.FirstOrDefault(q => q.Value == disMatrix.Values.Min()).Key, disMatrix.Values.Min());
            }

            tmpsortedMonstersList = Sort(inputList, level2Decision);
        }
        else if(level2Decision == "Lowest Attacking Ability")
        {
            foreach(var item in MonstersList)
            {
                inputList.Add(item.name, item.agentEnergy);
            }

            tmpsortedMonstersList = Sort(inputList, level2Decision);
        }
        else if(level2Decision == "Highest Attacking Ability")
        {
            foreach(var item in MonstersList)
            {
                inputList.Add(item.name, item.agentEnergy);
            }

            tmpsortedMonstersList = Sort(inputList, level2Decision);
        }
        else
        {
            Debug.Log("Explorer's level2 decision has something problem!");
        }

        foreach(var item1 in tmpsortedMonstersList)
        {
            foreach(var item2 in MonstersList)
            {
                if(item1.Key == item2.ID)
                {
                    sortedMonstersList.Add(item1.Key, item2.agentPos);
                }
            }
        }

        return sortedMonstersList;
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

        if(level2Decision == "Highest Attacking Ability")
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
            temp = temp.Insert(0, "Monster");
            sortedList.Add(temp, item.Value);
        }

        return sortedList;
    }

    // divide the entire group into several sub-groups according to the selection plan and get the specifical sub-group's final position
    private Vector3 GetSubGroupPos(Vector3 currentPos, Vector3 taskPos, int numGroups, int groupNum)
    {
        Vector3 translationPos = new Vector3();

        translationPos = currentPos + new Vector3(gameObject.GetComponent<AgentState>().agentRadius * 40 * (groupNum - 1) / numGroups, 0f, 0f);

        return translationPos;
    }

    // Compute the distance matrix and use the consensus queue to sequentially choose task.
    private void GetFinalDisPos(Dictionary<string, List<float>> robotsTaskDis, Dictionary<string, List<Vector3>> robotsTaskPos, Vector3 position, int groupNum)
    {
        int myPointIndex = 0; // which specific point agent choose
        Vector3 tmpFinalPos = new Vector3();

        robotsFinalPos = new Dictionary<string , Vector3>();
        finalDisList = new List<float>();

        List<string> tmp = new List<string>(robotsTaskDis.Keys);

        for(int i = 0; i < robotsTaskDis.Count; i++)
        {
            int posChosen = robotsTaskDis[tmp[i]].IndexOf(robotsTaskDis[tmp[i]].Min());

            myPointIndex = posChosen;

            finalDisList.Add(robotsTaskDis[tmp[i]][myPointIndex]);
            finalPosList.Add(robotsTaskPos[tmp[i]][myPointIndex]);
            robotsFinalPos.Add(tmp[i], robotsTaskPos[tmp[i]][myPointIndex]);

            for(int j = 0; j < robotsTaskDis.Count; j++)
            {
                robotsTaskDis[tmp[j]][myPointIndex] = 999999f;
            }
        }
        robotsFinalPos.TryGetValue(player, out tmpFinalPos);

        agentFinalPos = GetSubGroupPos(tmpFinalPos, position, gameObject.GetComponent<Selection>().numTask, groupNum);
    }
}
