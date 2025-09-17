using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DemographicsGroup", menuName = "ScriptableObjects/Demographics Group", order = 2)]
public class DemographicGroup : ScriptableObject
{
    [SerializeField] public PedestrianController[] pedestrians;
}
