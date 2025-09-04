using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReset : MonoBehaviour
{

    [SerializeField] private bool sendBelow = true;
    [SerializeField] private bool reportEvent = true;
    [SerializeField] private bool resetTrial = true;
    
    private void OnTriggerEnter(Collider other) {
        // Debug.Log("Collision with trigger detected");
        if (other.gameObject.GetComponent<EVRA_CharacterController>() != null || other.gameObject.layer == 7 || other.gameObject.layer == 3) {
            // Debug.Log("Collision with player detected");
            if (sendBelow) StreetSim.S.FailTrial();
            if (reportEvent) EEGStreetSim.ESS.WriteLine("Collision", other.transform.position, gameObject.name);
            if (resetTrial) StreetSim.S.ResetTrial();
            return;
        }
    }
}
