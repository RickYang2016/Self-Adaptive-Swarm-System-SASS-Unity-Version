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
    public Vector3 agentPos;
    public Vector3 safetyPos;
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
    }

    // Update is called once per frame
    void Update()
    {
        // agentEnergy = gameObject.GetComponent<Routing>().currentAgentEnergy;
        agentHP = gameObject.GetComponentInChildren<ExplorerAttackingState>().currentHP;

        level1Decision = gameObject.GetComponent<ImportDecisionLevel>().explorerLevel1Decision;

        if(gameObject.GetComponent<ImportDecisionLevel>().explorerLevel1Decision == "")
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

        safetyPos = new Vector3(tmp * 5, 0.25f, 0f);

        return safetyPos;
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
