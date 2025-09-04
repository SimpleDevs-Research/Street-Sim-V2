using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRegion : MonoBehaviour
{
    public float density; //Person per unit
    public float dirtiness;
    public float risk;

    public List<Pedestrian> peoplewithin = new List<Pedestrian>();
    public List<PathQualityEffector> effectors = new List<PathQualityEffector>();
    public float size;
    void Start()
    {
        size = transform.localScale.x * transform.localScale.z;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Pedestrian>())
        {
            peoplewithin.Add(other.GetComponent<Pedestrian>());
        }
        if(other.GetComponent<PathQualityEffector>())
        {
            effectors.Add(other.GetComponent<PathQualityEffector>());
            if(other.GetComponent<PathQualityEffector>().myEffect == PathQualityEffector.effectType.cleanliness)
            {
                dirtiness += other.GetComponent<PathQualityEffector>().effectLevel;
            }
            if (other.GetComponent<PathQualityEffector>().myEffect == PathQualityEffector.effectType.safety)
            {
                risk += other.GetComponent<PathQualityEffector>().effectLevel;
            }
        }
        density = peoplewithin.Count / size;

    }
    public void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Pedestrian>() && peoplewithin.Contains(other.GetComponent<Pedestrian>()))
        {
            peoplewithin.Remove(other.GetComponent<Pedestrian>());
        }
        if (other.GetComponent<PathQualityEffector>() && effectors.Contains(other.GetComponent<PathQualityEffector>()))
        {
            effectors.Remove(other.GetComponent<PathQualityEffector>());
            if(other.GetComponent<PathQualityEffector>().myEffect == PathQualityEffector.effectType.cleanliness) { 
                dirtiness -= other.GetComponent<PathQualityEffector>().effectLevel;
            }
            if (other.GetComponent<PathQualityEffector>().myEffect == PathQualityEffector.effectType.safety)
            {
                risk -= other.GetComponent<PathQualityEffector>().effectLevel;
            }
        }
        density = peoplewithin.Count / size;

    }
}
