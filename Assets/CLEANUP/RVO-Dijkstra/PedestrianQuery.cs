using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class PedestrianQuery : MonoBehaviour
{
    public float sumRiskAversion;
    public float sumDirtinessAversion;
    public float sumCrowdednessAversion;
    public float sumDistanceAversion;

    public List<Pedestrian> peoplewithin = new List<Pedestrian>();
    public float size;
    void Start()
    {
        size = transform.localScale.x * transform.localScale.z;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Pedestrian>())
        {
            peoplewithin.Add(other.GetComponent<Pedestrian>());
            Pedestrian.PedPersonality personalityData = other.GetComponent<Pedestrian>().m_personality;
            sumRiskAversion += personalityData.riskAversion;
            sumDirtinessAversion += personalityData.dirtinessAversion;
            sumCrowdednessAversion += personalityData.crowdednessAversion;
            sumDistanceAversion += personalityData.distanceAversion;
        }


    }
    public void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Pedestrian>() && peoplewithin.Contains(other.GetComponent<Pedestrian>()))
        {
            peoplewithin.Remove(other.GetComponent<Pedestrian>());
            Pedestrian.PedPersonality personalityData = other.GetComponent<Pedestrian>().m_personality;
            sumRiskAversion -= personalityData.riskAversion;
            sumDirtinessAversion -= personalityData.dirtinessAversion;
            sumCrowdednessAversion -= personalityData.crowdednessAversion;
            sumDistanceAversion -= personalityData.distanceAversion;
        }


    }

    private void OnDrawGizmos()
    {

        Handles.Label(transform.position + Vector3.up * 5, "Crowdedness Aversion: " + (peoplewithin.Count == 0 ? 0 :sumCrowdednessAversion / peoplewithin.Count).ToString("0.000"));
        Handles.Label(transform.position + Vector3.up * 5.4f, "Distance Aversion: " + (peoplewithin.Count == 0 ? 0 : sumDistanceAversion / peoplewithin.Count).ToString("0.000"));
        Handles.Label(transform.position + Vector3.up * 5.8f, "Dirtiness Aversion: " + (peoplewithin.Count == 0 ? 0 :sumDirtinessAversion / peoplewithin.Count).ToString("0.000"));
        Handles.Label(transform.position + Vector3.up * 6.2f, "Safety Aversion: " + (peoplewithin.Count == 0 ? 0 :sumDirtinessAversion / peoplewithin.Count).ToString("0.000"));
        Handles.Label(transform.position + Vector3.up * 6.6f, "Num Pedestrians: " + (peoplewithin.Count).ToString());
    }
}