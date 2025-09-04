using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class RouteNode : MonoBehaviour
{
    public float acceptableRadius;
    public List<RouteNode> connections = new List<RouteNode>();


    // Start is called before the first frame update
    void Awake()
    {
        acceptableRadius = Mathf.Infinity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   
}
