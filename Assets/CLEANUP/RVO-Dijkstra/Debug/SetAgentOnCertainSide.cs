using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAgentOnCertainSide : MonoBehaviour
{
    public bool SameSide;
    public float xExtent;
    void Start()
    {
        float mult = (SameSide ? 1 : -1) * Mathf.Sign(PlayerTracker.Instance.transform.position.x);

        transform.position = new Vector3(xExtent * mult, transform.position.y, transform.position.z);
    }
}
