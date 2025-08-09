using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemyHeadRename : MonoBehaviour
{
    public void Start()
    {
        gameObject.name = "Head-" + transform.parent.parent.parent.parent.parent.name;
        GetComponent<ReplayPositionNotifier>()._name = gameObject.name;
    }
}
