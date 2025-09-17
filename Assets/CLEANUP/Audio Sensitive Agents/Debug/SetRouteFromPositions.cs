using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRouteFromPositions : MonoBehaviour
{
    public Vector3 endPosition;
    void Start()
    {
        PedestrianController ped = GetComponent<PedestrianController>();
        ped.SetRouteStart(RouteManager.instance.GetNearestNode(transform.position));
        ped.SetRouteDestination(RouteManager.instance.GetNearestNode(endPosition));
    }
}
