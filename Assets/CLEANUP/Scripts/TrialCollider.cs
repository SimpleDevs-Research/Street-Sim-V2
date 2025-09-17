using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialCollider : MonoBehaviour
{
    public GameObject otherCollider;

    private void OnTriggerEnter(Collider other) {
        AudioSensitiveAgentsTrialController.Instance.TrialCollision();
        otherCollider.SetActive(true);
        gameObject.SetActive(false);
    }
}
