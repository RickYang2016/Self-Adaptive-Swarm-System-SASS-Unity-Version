using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class MonsterState : MonoBehaviour
{
    public string ID;
    public float agentEnergy;
    public float agentHP;
    public float agentRadius;
    public float agentSpeed;
    public float unitAttackingEnergyCost;
    public Vector3 agentPos;
    public Vector3 safetyPos;
    public string level1Decision;
    public string level2Decision;
    public string level3Decision;

    // Start is called before the first frame update
    private void Start()
    {
        ID = gameObject.name;
        agentRadius = 5f;
        safetyPos = GetSafetyPos(ID);
        agentSpeed = 15.0f;
        unitAttackingEnergyCost = 0.3f;
        level2Decision = "Nearest";
    }

    // Update is called once per frame
    void Update()
    {
        agentEnergy = gameObject.GetComponent<MonsterRouting>().currentAgentEnergy;
        agentHP = gameObject.GetComponentInChildren<MonsterAttackingState>().currentHP;
    }

    private Vector3 GetSafetyPos(string ID)
    {
        float tmp = 0;
        Vector3 safetyPos = new Vector3();

        string tmps = Regex.Replace(ID, "[a-z]", "", RegexOptions.IgnoreCase);
        float.TryParse(tmps, out tmp);

        safetyPos = new Vector3(tmp * -30f, 0.25f, tmp * -30);

        return safetyPos;
    }
}
