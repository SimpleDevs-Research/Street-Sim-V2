using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachAgentHelper : MonoBehaviour
{
    public bool triggered = false;
    void Start()
    {
        AudioSensitiveAgentsTrialController.Instance.onTrialChanged += onTrialChanged;
    }

    public void onTrialChanged()
    {
        if(!triggered)
        {
            triggered = true;

            PedestrianController ped = GetComponent<PedestrianController>();
            ped.SetRouteStart(RouteManager.instance.GetNearestNode(transform.position));
            ped.SetRouteDestination(RouteManager.instance.GetNearestNode(new Vector3(11, 0, 6)));
            ped.ResetRoute();
            ped.SetGoal(PedestrianController.Goal.TRAVEL);
        }
    }
}
