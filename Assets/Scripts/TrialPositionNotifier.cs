using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialPositionNotifier : MonoBehaviour
{
    public int _guid;
    public string _name;

    private void Awake() {
        _name = this.gameObject.name;
        _guid = this.gameObject.GetInstanceID();
    }

    private void LateUpdate() {
        if (TrialController.current != null) TrialController.current.UpdatePosition(Time.time, Time.frameCount, _name, _guid, transform.position, transform.forward);
    }
}
