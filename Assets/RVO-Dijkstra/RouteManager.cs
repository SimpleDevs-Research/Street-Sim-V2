using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public enum EnvironmentType
{
    Sidewalk,
    Crosswalk
}
[System.Serializable]
public class Route
{
    [Header("=== Manual ===")]
    public RouteNode node1;
    public RouteNode node2;
    public float pathWidth = 5;
    public float baseCost = 1;

    public float safety = 1;

    [Header("=== Computed ===")]
    public float distance;
    public float density;
    public float computedCost;
    public float dirtiness = 0;

    public PathRegion pathRegion;
}
public class RouteManager : MonoBehaviour
{
    [Header("=== References ===")]
    public List<Route> routes;
    public static RouteManager instance;

    public List<RouteNode> nodes;
    public float[,] edges;

    public Transform nodesParent;

    public RouteNode startDebug;
    public RouteNode endDebug;
    public bool runRoute;
    public GameObject pathRegion;

    [Header("=== Levers ===")]
    public bool considerRisk;
    public bool considerCrowding;
    public bool considerDirtiness;
    public bool useDjikstras;

    [Header("=== Weights ===")]
    public float distanceWeight = 1;
    public float complexityWeight = 1;
    public float safetyWeight = 1;
    public float densityWeight = 1;
    public float conditionWeight = 1;

    bool drawResults = false;
    float[] resultsSet = new float[0];

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        foreach(Route route in routes)
        {
            GameObject thePathRegion = Instantiate<GameObject>(pathRegion);
            thePathRegion.transform.position = (route.node1.transform.position + route.node2.transform.position) / 2;
            //thePathRegion.transform.rotation = Quaternion.Euler(
            //                                               new Vector3(0, Vector2.Angle(Vec3To2(route.node1.transform.position), Vec3To2(route.node2.transform.position)), 0));
            thePathRegion.transform.LookAt(route.node2.transform.position);
            thePathRegion.transform.localScale = new Vector3(route.pathWidth*2, 1, Vector3.Distance(route.node1.transform.position, route.node2.transform.position));
            route.pathRegion = thePathRegion.GetComponent<PathRegion>();
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        /*if(runRoute)
        {
            List<RouteNode> path = getRoute(startDebug, endDebug, null);
            String pathString = path[0].gameObject.name;
            for(int i = 1; i < path.Count; i++)
            {
                pathString += " -> " + path[i].gameObject.name;
            }
            print(pathString);
            runRoute = false;
        }*/
    } //

    public void recomputeRoutes(Pedestrian.PedPersonality personalityData)
    {
        foreach(Route route in routes)
        {
            route.distance = Vector3.Distance(route.node1.transform.position, route.node2.transform.position);
            route.dirtiness = route.pathRegion.dirtiness;
            route.safety = route.pathRegion.risk;
            route.density = route.pathRegion.density;
            route.computedCost = route.baseCost
                                + distanceWeight * route.distance * personalityData.distanceAversion
                                + densityWeight * route.density * route.distance * personalityData.crowdednessAversion * (considerCrowding ? 1 : 0)
                                + safetyWeight * route.safety * personalityData.riskAversion * (considerRisk ? 1 : 0)
                                + conditionWeight * route.dirtiness * personalityData.dirtinessAversion * (considerDirtiness ? 1 : 0);
        }
    }
    public void recomputeEdges(Pedestrian.PedPersonality personalityData)
    {
        recomputeRoutes(personalityData);
        edges = new float[nodesParent.childCount, nodesParent.childCount];
        for (int i = 0; i < nodesParent.childCount; i++)
        {
            for (int ii = 0; ii < nodesParent.childCount; ii++)
            {
                edges[i, ii] = -1;
            }
        }

        foreach (Route route in routes)
        {
            if (!nodes.Contains(route.node1))
            {
                nodes.Add(route.node1);
            }
            if (!nodes.Contains(route.node2))
            {
                nodes.Add(route.node2);
            }

            route.node1.acceptableRadius = Mathf.Min(route.node1.acceptableRadius, route.pathWidth);
            route.node2.acceptableRadius = Mathf.Min(route.node2.acceptableRadius, route.pathWidth);

            int ind1 = nodes.IndexOf(route.node1);
            int ind2 = nodes.IndexOf(route.node2);

            edges[ind1, ind2] = route.computedCost;
            edges[ind2, ind1] = route.computedCost;
        }
    }

    public List<RouteNode> getRoute(RouteNode start, RouteNode end, Pedestrian.PedPersonality personalityData)
    {
        List<RouteNode> bestPath = new List<RouteNode>();
        recomputeEdges(personalityData);

        if (!useDjikstras)
        {
            bestPath.Add(start);
            if(start != end)
                bestPath.Add(end);
            return bestPath;
        }

        float[] minimumDistance = new float[nodes.Count];
        for (int i = 0; i < nodes.Count; i++) minimumDistance[i] = int.MaxValue;
        float[] distances = new float[nodes.Count];
        for (int i = 0; i < nodes.Count; i++) distances[i] = int.MaxValue;

        int[] prevNode = new int[nodes.Count];
        for (int i = 0; i < nodes.Count; i++) prevNode[i] = -1;
        distances[nodes.IndexOf(start)] = 0;
        int failsafe1 = 0;

        while (infCount(minimumDistance) > 0 && failsafe1 < 99)
        {
            failsafe1++;
            
            float currentBestDistance = int.MaxValue;
            int currentBestNodeInd = -1;
            for (int i = 0; i < distances.Length; i++)
            {
                if (distances[i] < currentBestDistance && minimumDistance[i] == int.MaxValue)
                {
                    currentBestNodeInd = i;
                    currentBestDistance = distances[i];
                }
            }
            int currentNode = currentBestNodeInd;

            minimumDistance[currentNode] = distances[currentNode];
            for(int i = 0; i < nodes.Count; i++)
            {
                if (edges[currentNode, i] == -1) continue;
                float possibleNewDistance = distances[currentNode] + edges[currentNode, i];
                if(possibleNewDistance < distances[i])
                {
                    distances[i] = possibleNewDistance;
                    prevNode[i] = currentNode;
                }
            }
        }
 
        bestPath.Add(end);
        int currentNodeOnBestPath = prevNode[nodes.IndexOf(end)];

        int failsafe2 = 0;
        while(currentNodeOnBestPath != -1 && failsafe2 < 10)
        {
            failsafe2++;
            bestPath.Insert(0, nodes[currentNodeOnBestPath]);
            currentNodeOnBestPath = prevNode[currentNodeOnBestPath];
        }


        resultsSet = minimumDistance;
        drawResults = true;

        return bestPath;
    }
    int infCount(float[] array)
    {
        int count = 0;
        for(int i = 0; i < array.Length; i++)
        {
            if (array[i] == int.MaxValue) count++;
        }
        return count;
    }
    int getMinDistanceNode(float[] distances)
    {
        float currentBestDistance = int.MaxValue;
        int currentBestNodeInd = -1;
        for(int i = 0; i < distances.Length; i++)
        {
            if (distances[i] < currentBestDistance)
            {
                currentBestNodeInd = i;
                currentBestDistance = distances[i];
            }
        }
        return currentBestNodeInd;
    }

    private void OnDrawGizmosSelected()
    {

        foreach (Route route in routes)
        {
            if(route.node1 != null && route.node2 != null) {

                Gizmos.color = Color.red;

                float dist = Vector3.Distance(route.node1.transform.position, route.node2.transform.position);
                Vector3 routeCenter = route.node1.transform.position + (route.node2.transform.position - route.node1.transform.position) / 2;

                Gizmos.DrawLine(route.node1.transform.position, route.node2.transform.position);
                Handles.Label(routeCenter, route.baseCost.ToString());

                Gizmos.color = Color.blue;

                Vector3 dir = (route.node2.transform.position - route.node1.transform.position).normalized;

                Gizmos.DrawLine(route.node1.transform.position + new Vector3(dir.z, 0, dir.x) * route.pathWidth, route.node2.transform.position + new Vector3(dir.z, 0, dir.x) * route.pathWidth);
                Gizmos.DrawLine(route.node1.transform.position - new Vector3(dir.z, 0, dir.x) * route.pathWidth, route.node2.transform.position - new Vector3(dir.z, 0, dir.x) * route.pathWidth);


            }
        }

        if(drawResults)
        {
            for(int i = 0; i < nodes.Count; i++)
            {
                Handles.Label(nodes[i].transform.position, resultsSet[i].ToString());

            }
        }

        /*
        Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(transform.position, acceptableRadius);
        if (connections == null || costs == null) return;
        if (connections.Count == costs.Count)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, connections[i].transform.position);
                Gizmos.color = Color.black;
                Handles.Label(transform.position + (connections[i].transform.position - transform.position) / 2, costs[i].ToString());
            }
        }*/

    }
    public Vector2 Vec3To2(Vector3 vec)
    {
        return new Vector2(vec.x, vec.z);
    }
}
