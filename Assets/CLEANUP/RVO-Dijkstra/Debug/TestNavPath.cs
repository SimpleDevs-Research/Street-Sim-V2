using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestNavPath : MonoBehaviour
{
    //private NavMeshAgent agent;
    public Transform target;
    public NavMeshPath m_navPath;
    public List<Vector3> m_pathPositions;
    // Start is called before the first frame update
    void Start()
    {
        //agent = GetComponent<NavMeshAgent>();
        //bool success = agent.SetDestination(target.position);
        //Debug.Log("Found test path? " + success.ToString());

        m_navPath = new NavMeshPath();
        print("repathing");
        m_pathPositions = new List<Vector3>();
        bool pathFound = NavMesh.CalculatePath(
            transform.position,
            target.position,
            NavMesh.AllAreas,
            m_navPath
        );
        if (pathFound)
        {
            print("pathfound");
            NavMeshHit hit;
            foreach (Vector3 p in m_navPath.corners)
            {
                if (NavMesh.FindClosestEdge(p, out hit, NavMesh.AllAreas))
                {
                    m_pathPositions.Add(p);
                }
            }
        }
        else print("failure");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
