using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialCollider : MonoBehaviour
{
    public GameObject otherCollider;
    public int index;

    private void OnTriggerEnter(Collider other) {
        AudioSensitiveAgentsTrialController.Instance.TrialCollision(index, otherCollider);
        gameObject.SetActive(false);   
    }
}
