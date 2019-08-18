using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.IO;

[System.Serializable]
public class MonsterDecision
{
    public MonsterLevel1Decision level_1_decision;
    public MonsterLevel2Decision level_2_decision;
    public MonsterLevel3Decision level_3_decision;
}

[System.Serializable]
public class MonsterLevel1Decision
{
    public string attack;
    public string patrol;
    public string defend;
}

[System.Serializable]
public class MonsterLevel2Decision
{
    public string nearest;
    public string lowest_attacking_ability;
    public string highest_attacking_ability;
}

[System.Serializable]
public class MonsterLevel3Decision
{
    public string Independent;
    public string Dependent;
}

public class ImportMonDecLevel : MonoBehaviour
{
    public string monsterLevel1Decision;
    public string monsterLevel2Decision;
    public string monsterLevel3Decision;
    public int decisionTimes = 0;
    private MonsterDecision loadedData;
    private int newNumExplorers;
    private int oldNumExplorers;
    private int newChangeNumExplorers;
    private int oldChangeNumExplorers;
    private string explorersListPath = "/home/herobot/Documents/research/code/monsterDecExplorersList.txt";
    private string monstersListPath = "/home/herobot/Documents/research/code/monsterDecMonstersList.txt";
    private string monsterDecisionPath = "/home/herobot/Documents/research/code/MonstersDecision.json";

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        newNumExplorers = gameObject.GetComponent<MonsterCom>().perceptionList.Count;
        newChangeNumExplorers = newNumExplorers - oldNumExplorers;

        if(gameObject.GetComponent<MonsterCom>().perceptionList.Count != 0)
        {
            saveEStore(GetDataFromExplorers(gameObject.GetComponent<MonsterCom>().perceptionList));
            saveMStore(GetDataFromMonsters(gameObject.GetComponent<MonsterState>()));
        }

        // if MonstersList has changed and changing times are not equal, recalculate the decision
        if(newChangeNumExplorers != 0 && newChangeNumExplorers != oldChangeNumExplorers)
        {
            // update agent's energy state
            gameObject.GetComponent<MonsterState>().agentEnergy = gameObject.GetComponent<MonsterRouting>().currentAgentEnergy;

            decisionTimes++;
            // RunPythonInCs();

            // update decision results
            LoadDecisionResult();          
        }
        else if(newNumExplorers == 0)
        {
            monsterLevel1Decision = "Patroling";
            monsterLevel2Decision = "Nearest";
            monsterLevel3Decision = "Independent";           
        }

        oldChangeNumExplorers = newChangeNumExplorers;
        oldNumExplorers = newNumExplorers;        
    }

    // run python code in C#
    private void RunPythonInCs()
    {
        string tmp = "python /home/herobot/Documents/research/code/SGDT_version_2_Monster.py";

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
        print(strResult);
        p.Close();
    }

    // get two opponent groups average distance
    private float GetAverageDis()
    {
        float averageDis = 0;
        List<float> disMatrix  = new List<float>();;

        if(gameObject.GetComponent<MonsterCom>().perceptionList != null)
        {
            foreach(var item in gameObject.GetComponent<MonsterCom>().perceptionList)
            {
                disMatrix.Add(Vector3.Distance(gameObject.GetComponent<MonsterState>().agentPos, item.agentPos));
            }
        }

        averageDis = disMatrix.Sum() / gameObject.GetComponent<MonsterCom>().perceptionList.Count;

        return averageDis;
    }

    // get all the perceptionList data to list
    private List<List<float>> GetDataFromExplorers(List<perceptiveExplorerState> perceptionList)
    {
        List<List<float>> agentsInfo = new List<List<float>>();

        foreach(var item in perceptionList)
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

    // get Monster's data to list
    private List<List<float>> GetDataFromMonsters(MonsterState Monster)
    {
        List<List<float>> agentsInfo = new List<List<float>>();
        List<float> agentInfo = new List<float>();

        agentInfo.Add(Monster.unitAttackingEnergyCost);
        agentInfo.Add(Monster.agentEnergy);
        agentInfo.Add(Monster.agentHP);
        agentInfo.Add(Convert.ToSingle(Math.Pow(Monster.agentRadius, 2.0) * Math.PI));
        agentInfo.Add(Monster.agentSpeed);
        agentInfo.Add(GetAverageDis());

        agentsInfo.Add(agentInfo);
        
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
        if(File.Exists(monsterDecisionPath))
        {
            string dataJson = File.ReadAllText(monsterDecisionPath);

            loadedData = JsonUtility.FromJson<MonsterDecision>(dataJson);
        }

        // get level 1 decision
        if(Convert.ToSingle(loadedData.level_1_decision.patrol) != 0)
        {
            monsterLevel1Decision = "Patroling";
        }
        else if(Convert.ToSingle(loadedData.level_1_decision.attack) != 0)
        {
            monsterLevel1Decision = "Attacking";
        }
        else if(Convert.ToSingle(loadedData.level_1_decision.defend) != 0)
        {
            monsterLevel1Decision = "Defending";
        }
        else
        {
            print("Load monster decision level 1 has some problem!");
        }

        // get level 2 decision
        if(Convert.ToSingle(loadedData.level_2_decision.nearest) != 0)
        {
            monsterLevel2Decision = "Nearest";
        }
        else if(Convert.ToSingle(loadedData.level_2_decision.lowest_attacking_ability) != 0)
        {
            monsterLevel2Decision = "Lowest Attacking Ability";
        }
        else if(Convert.ToSingle(loadedData.level_2_decision.highest_attacking_ability) != 0)
        {
            monsterLevel2Decision = "Highest Attacking Ability";
        }
        else
        {
            print("Load monster decision level 2 has some problem!");
        }

        // get level 3 decision
        if(Convert.ToSingle(loadedData.level_3_decision.Independent) != 0)
        {
            monsterLevel3Decision = "Independent";
        }
        else if(Convert.ToSingle(loadedData.level_3_decision.Dependent) != 0)
        {
            monsterLevel3Decision = "Dependent";
        }
        else
        {
            print("Load monster decision level 3 has some problem!");
        }
    }

    // get the decision results depending on different scenarios
    private void GetDecisionResult()
    {

    }
}
