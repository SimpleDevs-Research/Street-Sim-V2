using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemyRename : MonoBehaviour
{
    public void Awake()
    {
        gameObject.name = "Remy-" + transform.parent.name;
        GetComponent<TrialPositionNotifier>()._name = gameObject.name;
    }
}
