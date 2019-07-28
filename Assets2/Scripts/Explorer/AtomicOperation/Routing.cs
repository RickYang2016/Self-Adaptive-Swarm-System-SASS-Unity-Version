using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using System;

public class Routing : MonoBehaviour
{
    public List<string> before = new List<string>();
    public List<string> after = new List<string>();
    public float step;
    public double BoundaryAngle;
    public double relativeAngle;
    public Vector3 currentMoveDirection;
    public Queue<Vector3> targetQueue;
    public List<string> sortedCollisionList;
    private bool isDeadLock = false;
    private Vector3 agentOldPos;
    private Vector3 agentNewPos;
    private List<List<string>> deadLockDetectList = new List<List<string>>();
    private Dictionary<string, float> sortedCollisionDic;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        agentNewPos = gameObject.GetComponent<AgentState>().agentPos;
        sortedCollisionDic = new Dictionary<string, float>();

        // initialize target point
        step = gameObject.GetComponent<AgentState>().agentSpeed * Time.deltaTime;
        targetQueue = new Queue<Vector3>();
        targetQueue.Enqueue(gameObject.GetComponent<Formation>().agentFinalPos);

        // whether or not have collision
        if(gameObject.GetComponentInChildren<CollisionDection>().CollisionList.Count != 0)
        {
            // Debug.Log("enter " + currentMoveDirection);
            Dictionary<string, float> tmpColList = new Dictionary<string, float>();

            foreach(var item in gameObject.GetComponentInChildren<CollisionDection>().CollisionList)
            {
                tmpColList.Add(item.name, item.agentEnergy);
            }

            // get sorted collision Dictionary
            // sortedCollisionDic = Sort(tmpColList);

            // whether or not have deadlock
            if(isDeadLock == false)
            {
                sortedCollisionDic = Sort(tmpColList);
                DeadLockDetect(sortedCollisionDic);
            }

            while(isDeadLock == true && sortedCollisionDic.Count != 0)
            {
                before = new List<string>();
                after  = new List<string>();

                foreach(var item in sortedCollisionDic)
                {
                    before.Add(item.Key);
                }

                sortedCollisionDic = DeadLockSolve(sortedCollisionDic);

                CollisionRoutingPlan(sortedCollisionDic);

                foreach(var item in sortedCollisionDic)
                {
                    after.Add(item.Key);
                }

                // MoveTo(sortedCollisionDic);

                DeadLockDetect(sortedCollisionDic);
            }

            CollisionRoutingPlan(sortedCollisionDic);

            // MoveTo(sortedCollisionDic);
        }
        else
        {
            gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition, targetQueue.Dequeue(), step);
        }

        currentMoveDirection = agentNewPos - agentOldPos;
        agentOldPos = agentNewPos;
    }

    private void DeadLockDetect(Dictionary<string, float> sortedCollisionDic)
    {
        List<string> sortedCollisionList = new List<string>();

        foreach(var item in sortedCollisionDic)
        {
            sortedCollisionList.Add(item.Key);
        }

        deadLockDetectList.Add(sortedCollisionList);

        // set sepcifical step to judge deadlock whether or not exit
        if(deadLockDetectList.Count == 10)
        {
            Debug.Log(deadLockDetectList.Count);

            if(deadLockDetectList[0].Count == deadLockDetectList[deadLockDetectList.Count-1].Count
               && deadLockDetectList[0].Count(t => !deadLockDetectList[deadLockDetectList.Count-1].Contains(t)) == 0)
            {
                isDeadLock = true;
            }

            deadLockDetectList.Clear();
        }
        else
        {
            isDeadLock = false;
        }

        Debug.Log(isDeadLock);
    }

    private Dictionary<string, float> DeadLockSolve(Dictionary<string, float> sortedCollisionDic)
    {
        Dictionary<string, float> resortedCollisionDic = new Dictionary<string, float>();

        List<string> tmpKeys = sortedCollisionDic.Keys.ToList();
        List<float> tmpValues = sortedCollisionDic.Values.ToList();

        // switch the first element and the last element in the sorted collision dictionary
        tmpKeys = Rotation<string>(tmpKeys, 0, tmpKeys.Count);
        tmpValues = Rotation<float>(tmpValues, 0, tmpValues.Count);
        resortedCollisionDic = tmpKeys.Zip(tmpValues, (a, b) => new {a, b}).ToDictionary(p => p.a, p => p.b);

        return resortedCollisionDic;
    }

    private List<T> Rotation<T>(List<T> list, int indexA, int indexB)
    {
        T tmp = list[indexA];
        list.Insert(indexB, tmp);
        list.Remove(list[indexA]);

        return list;
    }

    private void MoveTo(Dictionary<string, float> sortedCollisionDic)
    {
        if(sortedCollisionDic.Keys.First() == gameObject.name)
        {
            Debug.Log("enter " + currentMoveDirection);                
            // move to specificial point
            CollisionRoutingPlan(sortedCollisionDic);
        }
        else
        {
            gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

    private void CollisionRoutingPlan(Dictionary<string, float> sortedCollisionDic)
    {
        float stepParameter = 5;
        double leftDeflectionAngle = 0;
        double rightDeflectionAngle = 0;
        double deflectionAngle = 30;
        Vector3 nearestAgentPos = new Vector3();
        Vector3 relativeDirection = new Vector3();
        Vector3 dirIdentification = new Vector3();
        Dictionary<string, float> disMatrix = new Dictionary<string, float>();
        Dictionary<string, float> tmpdisMatrix = new Dictionary<string, float>();
        Dictionary<string, float> tmpDic;

        BoundaryAngle = 0f;

        // calculate the distance matrix between current agent and other agents in the collision list
        foreach(var item1 in sortedCollisionDic)
        {
            if(item1.Key != gameObject.name)
            {
                foreach(var item2 in gameObject.GetComponentInChildren<CollisionDection>().CollisionList)
                {
                    if(item1.Key == item2.ID)
                    {
                        disMatrix.Add(item1.Key, Vector3.Distance(gameObject.GetComponent<AgentState>().agentPos,
                                        item2.GetComponentInParent<CollisionSensorState>().agentPos));
                        break;
                    }
                }
            }
        }

        tmpdisMatrix = Sort(disMatrix);

        // avoid the collision to the nearest agent in the collision list
        // calculate the collision boundary between itself and nearest one
        BoundaryAngle = Math.Atan(2 * gameObject.GetComponent<AgentState>().agentRadius / 
                                    (tmpdisMatrix.Values.First() - 2 * gameObject.GetComponent<AgentState>().agentRadius)) / Math.PI * 180;

        // get the nearest agent's position
        foreach(var item in gameObject.GetComponentInChildren<CollisionDection>().CollisionList)
        {
            if(tmpdisMatrix.Keys.First() == item.ID)
            {
                nearestAgentPos = item.agentPos;
            }
        }

        relativeDirection = nearestAgentPos - gameObject.GetComponent<AgentState>().agentPos;
        dirIdentification = Vector3.Cross(currentMoveDirection, relativeDirection);
        relativeAngle = Vector3.Angle(currentMoveDirection, relativeDirection);

        // confirm the current direction in which side of two agents center points direction
        if(dirIdentification.y > 0)
        {
            // current direction is in the left side
            leftDeflectionAngle = BoundaryAngle - relativeAngle + deflectionAngle;
            rightDeflectionAngle = BoundaryAngle + relativeAngle + deflectionAngle;
        }
        else if(dirIdentification.y == 0)
        {
            // current direction is in the same line
            leftDeflectionAngle = BoundaryAngle + deflectionAngle;
            rightDeflectionAngle = BoundaryAngle + deflectionAngle;
        }
        else
        {
            // current direction is in the right side
            leftDeflectionAngle = BoundaryAngle + relativeAngle + deflectionAngle;
            rightDeflectionAngle = BoundaryAngle - relativeAngle + deflectionAngle;
        }

        // move to another direction
        tmpDic = new Dictionary<string, float>();

        Vector3 nextStepPos = new Vector3();

        tmpDic = GetFinalDeflectionAngle(leftDeflectionAngle, rightDeflectionAngle);

        if(tmpDic.Keys.First() == "left")
        {
            // transform.Rotate(Vector3.up, tmpDic.Values.First());
            
            currentMoveDirection = Quaternion.AngleAxis(tmpDic.Values.First(), Vector3.up) * currentMoveDirection;
            // transform.Translate(currentMoveDirection, Space.World);

            nextStepPos = stepParameter * gameObject.GetComponent<AgentState>().agentRadius * currentMoveDirection.normalized + gameObject.transform.localPosition;
            gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition, nextStepPos, step);
        }
        else
        {
            // transform.Rotate(Vector3.down, tmpDic.Values.First());
            currentMoveDirection = Quaternion.AngleAxis(tmpDic.Values.First(), Vector3.down) * currentMoveDirection;
            // transform.Translate(currentMoveDirection, Space.World);

            nextStepPos = stepParameter * gameObject.GetComponent<AgentState>().agentRadius * currentMoveDirection.normalized + gameObject.transform.localPosition;
            gameObject.transform.localPosition = Vector3.MoveTowards(gameObject.transform.localPosition, nextStepPos, step);            
        }
    }

    // sort the dictionary based on the energy level or distance, first order by the value, if the values are same, then order by key.
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
            temp = temp.Insert(0, "Explorer");
            infoDicSort.Add(temp, item.Value);
        }

        return infoDicSort;
    }

    // confirm agent's deflection angle
    private Dictionary<string, float> GetFinalDeflectionAngle(double leftDeflectionAngle, double rightDeflectionAngle)
    {
        int rightSideAgents = 0;
        int leftSideAgents = 0;
        Dictionary<string, float> finalDeflectionAngle = new Dictionary<string, float>();
        Vector3 rotatedVector = new Vector3();

        rotatedVector = currentMoveDirection;

        foreach(var item in gameObject.GetComponentInChildren<CollisionDection>().CollisionList)
        {
            if(item.ID != gameObject.name)
            {
                if(Vector3.Cross(Quaternion.AngleAxis(Convert.ToSingle(rightDeflectionAngle), Vector3.up) * 
                   rotatedVector, item.agentPos - gameObject.GetComponent<AgentState>().agentPos).y > 0)
                {
                    rightSideAgents++;
                }

                if(Vector3.Cross(Quaternion.AngleAxis(Convert.ToSingle(leftDeflectionAngle), Vector3.down) * 
                   rotatedVector, item.agentPos - gameObject.GetComponent<AgentState>().agentPos).y < 0)
                {
                    leftSideAgents++;
                }
            }
        }

        if(rightSideAgents > leftSideAgents)
        {
            finalDeflectionAngle.Add("left", Convert.ToSingle(leftDeflectionAngle));
        }
        else if(rightSideAgents == leftSideAgents)
        {
            finalDeflectionAngle.Add("right", Convert.ToSingle(rightDeflectionAngle));
        }
        else
        {
            finalDeflectionAngle.Add("right", Convert.ToSingle(rightDeflectionAngle));
        }

        return finalDeflectionAngle;
    }
}