using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialCollider : MonoBehaviour
{
    public Collider thisCollider;

    private void OnTriggerEnter(Collider other) {
        if (TrialController.current != null) TrialController.current.TrialCollision(thisCollider, other);
    }
}
