using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadAlignmentFix : MonoBehaviour
{

    public float adjustmentTime = 1f;
    public Transform headRef = null;
    public Transform newForwardRef = null;

    private float startTime;

    private void Awake() {
        startTime = Time.time;
    }

    private void Start() {
        if (headRef != null) StartCoroutine(Adjustment());
    }

    // Update is called once per frame
    private IEnumerator Adjustment() {
        while(Time.time - startTime <= adjustmentTime) {
            Vector3 newForward = headRef.forward;
            newForward.y = 0f;
            headRef.rotation = Quaternion.LookRotation(newForward, Vector3.up);
            if (newForwardRef != null) newForwardRef.position = headRef.position + Vector3.Scale(newForward,new Vector3(1f,1f,3f));
            yield return null;
        }
    }
}
