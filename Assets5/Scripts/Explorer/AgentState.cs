using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System;

public class AgentState : MonoBehaviour
{
    public string ID;
    public float agentEnergy;
    public float agentHP;
    public float agentRadius;
    public float agentSpeed;
    public float unitAttackingEnergyCost;
    public int numGroups;
    public int numAgents;
    public Vector3 agentPos;
    public Vector3 safetyPos;
    public Vector3 goalPos;
    public Vector3 centralGoalPos;
    public string level1Decision;
    public string level2Decision;
    public string level3Decision;
    public bool isFinished;

    // Start is called before the first frame update
    void Start()
    {
        ID = gameObject.name;
        agentEnergy = 90.0f;
        agentRadius = 3f;
        agentSpeed = 10.0f;
        unitAttackingEnergyCost = 0.1f;
        safetyPos = GetSafetyPos(ID);
        isFinished = false;
        centralGoalPos = new Vector3(-10, 0, 200);
    }

    // Update is called once per frame
    void Update()
    {
        // update agents' number in agent's state
        numAgents = gameObject.GetComponent<CommunicationFramework>().CommunicationList.Count;
        // update agent' goalPos in agent's state
        goalPos = GetGoalPos(ID);
        // update agent' HP in agent's state
        agentHP = gameObject.GetComponentInChildren<ExplorerAttackingState>().currentHP;
        // level1Decision = "Patroling";
        level1Decision = gameObject.GetComponent<ImportDecisionLevel>().explorerLevel1Decision;

        if(level1Decision == "")
        {
            level2Decision = "Nearest";
        }
        else
        {
            level2Decision = gameObject.GetComponent<ImportDecisionLevel>().explorerLevel2Decision;
        }

        level3Decision = gameObject.GetComponent<ImportDecisionLevel>().explorerLevel3Decision;
        numGroups = GetNumGroups(level3Decision);
    }

    private Vector3 GetSafetyPos(string ID)
    {
        float tmp = 0;
        Vector3 safetyPos = new Vector3();

        string tmps = Regex.Replace(ID, "[a-z]", "", RegexOptions.IgnoreCase);
        float.TryParse(tmps, out tmp);

        safetyPos = new Vector3(tmp * 5, 0.25f, -250f);

        return safetyPos;
    }

    private Vector3 GetGoalPos(string ID)
    {
        float tmp = 0;
        float taskRadius = 6 * agentRadius;
        float theta = 0;

        // set the initial point
        goalPos = centralGoalPos;

        string tmps = Regex.Replace(ID, "[a-z]", "", RegexOptions.IgnoreCase);
        float.TryParse(tmps, out tmp);

        if(numAgents != 0)
        {
            theta = Convert.ToSingle(Math.PI * 2 * tmp / numAgents);
            goalPos = goalPos + new Vector3(Convert.ToSingle(taskRadius * Math.Cos(theta)), 0, Convert.ToSingle(taskRadius * Math.Sin(theta)));
        }
        else
        {
            goalPos = new Vector3(10, 10, 10);
        }

        return goalPos;
    }

    private int GetNumGroups(string Level3Decision)
    {
        int tmp = 0;

        if(level3Decision == "One Group")
        {
            tmp = 1;
        }
        else if(level3Decision == "Two Groups")
        {
            tmp = 2;
        }
        else if(level3Decision == "Three Groups")
        {
            tmp = 3;
        }
        else
        {
            Debug.Log("Level 3 decision has some problem!");
        }

        return tmp;
    }
}
