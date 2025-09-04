using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DemographicsPreset", menuName = "ScriptableObjects/Demographics Preset", order = 1)]
public class DemographicsPreset : ScriptableObject
{
    [SerializeField] public DemographicGroup[] groups;
    [SerializeField] public float[] proportions;
}
