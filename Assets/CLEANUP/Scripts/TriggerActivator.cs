using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerActivator : MonoBehaviour
{
    public GameObject toActivate;
    private void OnTriggerEnter(Collider other)
    {
        toActivate.SetActive(true);
        gameObject.SetActive(false);
    }
}
