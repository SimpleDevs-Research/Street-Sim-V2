using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Writer : MonoBehaviour
{
    public static Writer current;
    public virtual void UpdatePosition(float t, int frame, string _name, int _guid, Vector3 p, Vector3 f)
    {
    }
}
