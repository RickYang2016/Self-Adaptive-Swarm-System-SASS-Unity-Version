using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.IO;

[System.Serializable]
public class Decision
{
    public Level1Decision level_1_decision;
    public Level2Decision level_2_decision;
    public Level3Decision level_3_decision;
}

[System.Serializable]
public class Level1Decision
{
    public string attack;
    public string patrol;
    public string defend;
}

[System.Serializable]
public class Level2Decision
{
    public string nearest;
    public string lowest_attacking_ability;
    public string highest_attacking_ability;
}

[System.Serializable]
public class Level3Decision
{
    public string one_group;
    public string two_groups;
    public string three_groups;
}

public class ImportDecisionLevel : MonoBehaviour
{
    public string explorerLevel1Decision;
    public string explorerLevel2Decision;
    public string explorerLevel3Decision;
    public int decisionTimes = 0;
    private Decision loadedData;
    private int newNumMonsters;
    private int oldNumMonsters;
    private int newChangeNumMonsters;
    private int oldChangeNumMonsters;
    private string explorersListPath = "/home/herobot/Documents/research/code/explorersList.txt";
    private string monstersListPath = "/home/herobot/Documents/research/code/monstersList.txt";
    private string explorersDecisionPath = "/home/herobot/Documents/research/code/explorersDecision.json";

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {   
        newNumMonsters = gameObject.GetComponent<CommunicationFramework>().MonstersList.Count;
        newChangeNumMonsters = newNumMonsters - oldNumMonsters;

        if(gameObject.GetComponent<CommunicationFramework>().MonstersList.Count != 0)
        {
            saveEStore(GetDataFromExplorers(gameObject.GetComponent<CommunicationFramework>().CommunicationList));
            saveMStore(GetDataFromMonsters(gameObject.GetComponent<CommunicationFramework>().MonstersList));
        }

        // if MonstersList has changed and changing times are not equal, recalculate the decision
        if(newChangeNumMonsters != 0 && newChangeNumMonsters != oldChangeNumMonsters)
        {
            // update agent's energy state
            gameObject.GetComponent<AgentState>().agentEnergy = gameObject.GetComponent<Routing>().currentAgentEnergy;

            decisionTimes++;
            // RunPythonInCs();  

            // update decision results
            LoadDecisionResult();          
        }
        else if(newNumMonsters == 0)
        {
            explorerLevel1Decision = "Patroling";
            explorerLevel2Decision = "Nearest";
            explorerLevel3Decision = "One Group";
        }

        oldChangeNumMonsters = newChangeNumMonsters;
        oldNumMonsters = newNumMonsters;
    }

    // run python code in C#
    private void RunPythonInCs()
    {
        string tmp = "python /home/herobot/Documents/research/code/SGDT_version_2.py";

        Process p = new Process();
        p.StartInfo.FileName = "sh";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        p.StandardInput.WriteLine(tmp);
        p.StandardInput.WriteLine("exit");
        string strResult = p.StandardOutput.ReadToEnd();
        // print(strResult);
        p.Close();
    }

    // get two opponent groups average distance
    private float GetAverageDis()
    {
        float averageDis = 0;
        List<float> disMatrix  = new List<float>();;

        if(gameObject.GetComponent<CommunicationFramework>().MonstersList != null)
        {
            foreach(var item1 in gameObject.GetComponent<CommunicationFramework>().MonstersList)
            {
                foreach(var item2 in gameObject.GetComponent<CommunicationFramework>().CommunicationList)
                {
                    disMatrix.Add(Vector3.Distance(item1.agentPos, item2.agentPos));
                }
            }
        }

        averageDis = disMatrix.Sum() / (gameObject.GetComponent<CommunicationFramework>().MonstersList.Count *
                                       gameObject.GetComponent<CommunicationFramework>().CommunicationList.Count);

        return averageDis;
    }

    // get all the Communicationlist data to list
    private List<List<float>> GetDataFromExplorers(List<AgentState> CommunicationList)
    {
        List<List<float>> agentsInfo = new List<List<float>>();

        foreach(var item in CommunicationList)
        {
            List<float> agentInfo = new List<float>();

            agentInfo.Add(item.unitAttackingEnergyCost);
            agentInfo.Add(item.agentEnergy);
            agentInfo.Add(item.agentHP);
            agentInfo.Add(Convert.ToSingle(Math.Pow(item.agentRadius, 2.0) * Math.PI));
            agentInfo.Add(item.agentSpeed);
            agentInfo.Add(GetAverageDis());

            agentsInfo.Add(agentInfo);
        }

        return agentsInfo;
    }

    // get all the MOnsterslist data to list
    private List<List<float>> GetDataFromMonsters(List<perceptiveMonsterState> MonstersList)
    {
        List<List<float>> agentsInfo = new List<List<float>>();

        foreach(var item in MonstersList)
        {
            List<float> agentInfo = new List<float>();

            agentInfo.Add(item.unitAttackingEnergyCost);
            agentInfo.Add(item.agentEnergy);
            agentInfo.Add(item.agentHP);
            agentInfo.Add(Convert.ToSingle(Math.Pow(item.agentRadius, 2.0) * Math.PI));
            agentInfo.Add(item.agentSpeed);
            agentInfo.Add(GetAverageDis());

            agentsInfo.Add(agentInfo);
        }

        return agentsInfo;
    }

    // store the explorers' data in file
    private void saveEStore(List<List<float>> agentsInfo)
    {
        StreamWriter inputPyExplorersData;

        inputPyExplorersData = new StreamWriter(explorersListPath, false);

        for(int i = 0; i < agentsInfo.Count; i++)
        {
            string data = " ";

            for(int j = 0; j < agentsInfo[i].Count; j++)
            {
                data += agentsInfo[i][j];

                if(j != agentsInfo[i].Count - 1)
                {
                    data += ",";
                }
            }

            inputPyExplorersData.WriteLine(data);            
        }

        inputPyExplorersData.Close();
        agentsInfo.Clear();
    }

    // store the monsters' data in file
    private void saveMStore(List<List<float>> agentsInfo)
    {
        StreamWriter inputPyMonstersData;

        inputPyMonstersData = new StreamWriter(monstersListPath, false);

        for(int i = 0; i < agentsInfo.Count; i++)
        {
            string data = " ";

            for(int j = 0; j < agentsInfo[i].Count; j++)
            {
                data += agentsInfo[i][j];

                if(j != agentsInfo[i].Count - 1)
                {
                    data += ",";
                }
            }

            inputPyMonstersData.WriteLine(data);            
        }

        inputPyMonstersData.Close();
        agentsInfo.Clear();
    }

    // update the decision results from decision level
    private void LoadDecisionResult()
    {
        if(File.Exists(explorersDecisionPath))
        {
            string dataJson = File.ReadAllText(explorersDecisionPath);

            loadedData = JsonUtility.FromJson<Decision>(dataJson);
        }

        // get level 1 decision
        if(Convert.ToSingle(loadedData.level_1_decision.patrol) != 0)
        {
            explorerLevel1Decision = "Patroling";
        }
        else if(Convert.ToSingle(loadedData.level_1_decision.attack) != 0)
        {
            explorerLevel1Decision = "Attacking";
        }
        else if(Convert.ToSingle(loadedData.level_1_decision.defend) != 0)
        {
            explorerLevel1Decision = "Defending";
        }
        else
        {
            print("Load decision level 1 has some problem!");
        }

        // get level 2 decision
        if(Convert.ToSingle(loadedData.level_2_decision.nearest) != 0)
        {
            explorerLevel2Decision = "Nearest";
        }
        else if(Convert.ToSingle(loadedData.level_2_decision.lowest_attacking_ability) != 0)
        {
            explorerLevel2Decision = "Lowest Attacking Ability";
        }
        else if(Convert.ToSingle(loadedData.level_2_decision.highest_attacking_ability) != 0)
        {
            explorerLevel2Decision = "Highest Attacking Ability";
        }
        else
        {
            print("Load decision level 2 has some problem!");
        }

        // get level 3 decision
        if(Convert.ToSingle(loadedData.level_3_decision.one_group) != 0)
        {
            explorerLevel3Decision = "One Group";
        }
        else if(Convert.ToSingle(loadedData.level_3_decision.two_groups) != 0)
        {
            explorerLevel3Decision = "Two Groups";
        }
        else if(Convert.ToSingle(loadedData.level_3_decision.three_groups) != 0)
        {
            explorerLevel3Decision = "Three Groups";
        }
        else
        {
            print("Load decision level 3 has some problem!");
        }
    }
}
