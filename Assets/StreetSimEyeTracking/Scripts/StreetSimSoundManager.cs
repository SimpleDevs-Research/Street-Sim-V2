using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(RemoteCollider))]
public class StreetSimSoundManager : MonoBehaviour
{

    private AudioSource _as;
    private RemoteCollider _rc;
    public int maxAgentsThreshold = 3;

    private void Awake() {
        _as = GetComponent<AudioSource>();
        _rc = GetComponent<RemoteCollider>();
    }

    // Update is called once per frame
    void Update() {
        _as.volume = (maxAgentsThreshold == 0) 
            ? 1f 
            : Mathf.Clamp(((float)_rc.numColliders)/((float)maxAgentsThreshold), 0f, 1f);
    }
}
