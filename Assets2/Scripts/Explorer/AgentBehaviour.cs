using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AgentBehaviour : MonoBehaviour
{
    public List<Collider> agents = new List<Collider>();
    private Rigidbody rigidBody;
    private AgentState agentState;
    public List<AgentState> agentsInfo;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // if(Input.GetKey(KeyCode.W))
        // {
        //     rigidBody.AddForce(new Vector3(0, 0, 1) * 1, ForceMode.Acceleration);
        // }
        // else if(Input.GetKey(KeyCode.S))
        // {
        //     rigidBody.AddForce(new Vector3(0, 0, 1) * -1, ForceMode.Acceleration);
        // }
        // else if(Input.GetKey(KeyCode.A))
        // {
        //     rigidBody.AddForce(new Vector3(1, 0, 0) * 1, ForceMode.Acceleration);
        // }
        // else if(Input.GetKey(KeyCode.D))
        // {
        //     rigidBody.AddForce(new Vector3(1, 0, 0) * -1, ForceMode.Acceleration);
        // }
        // else{}
        // // rigidBody.velocity = new Vector3(1, 0, 0) * 10;

        gameObject.GetComponent<AgentState>().agentPos = transform.position;

    }

    private void OnTriggerEnter(Collider agent)
    {
        // if(agent.gameObject.name == "Player2")
        // {
        //     print("Detect Monster! Be careful!");
        //     rigidBody.AddForce(new Vector3(1, 0, 10) * 1, ForceMode.Impulse);
        // }

        agentState = agent.GetComponent<AgentState>();

        // update agent's state and entire agents' list information
        if(agent.tag == "Explorer")
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

        // foreach(var item in agentsInfo)
        // {
        //     Debug.Log(item.ID);
        // }
    }

    private void OnTriggerExit(Collider agent)
    {
        agentState = agent.GetComponent<AgentState>();

        if(agent.tag == "Explorer")
        {
            agents.Remove(agent);
            agentsInfo.Remove(agentState);
        }
    }
}

