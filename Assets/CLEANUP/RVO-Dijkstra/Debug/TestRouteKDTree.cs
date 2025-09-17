using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRouteKDTree : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        List<int> resultIndices = new List<int>();

        RouteManager.instance.KNearestQuery(transform.position, 1, resultIndices);
        for (int i = 0; i < resultIndices.Count; i++)
            RouteManager.instance.nodes[resultIndices[i]].transform.localScale = Vector3.one * 2f;
    }
}
