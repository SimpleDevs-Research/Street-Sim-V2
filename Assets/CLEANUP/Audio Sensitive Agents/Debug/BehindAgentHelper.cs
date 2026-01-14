using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehindAgentHelper : MonoBehaviour
{
    public float xExtent;
    public bool spawned = false;
    void Start()
    {
        float mult = Mathf.Sign(PlayerTracker.Instance.transform.position.x);

        transform.position = new Vector3(xExtent * mult, transform.position.y, PlayerTracker.Instance.transform.position.z);
        transform.rotation = Quaternion.LookRotation(PlayerTracker.Instance.transform.position - transform.position, Vector3.up);
    }

    private void Update()
    {
        float diff = Vector3.Angle(transform.position - PlayerTracker.Instance.transform.position, PlayerTracker.Instance.transform.forward);
        if (diff > 130 && spawned == false)
        {
            spawned = true;
            transform.GetChild(0).gameObject.SetActive(true);

            PedestrianController ped = GetComponentInChildren<PedestrianController>();
            ped.SetRouteStart(RouteManager.instance.GetNearestNode(transform.position));
            ped.SetRouteDestination(RouteManager.instance.GetNearestNode(new Vector3( -Mathf.Sign(transform.position.x) * 20, 0, -6)));
            ped.ResetRoute();
        }
    }
}
