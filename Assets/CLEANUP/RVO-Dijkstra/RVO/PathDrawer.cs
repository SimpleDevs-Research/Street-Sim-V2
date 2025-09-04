using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathDrawer : MonoBehaviour
{

    private LineRenderer lr;
    private List<Vector3> trajPoints = new List<Vector3>();
    public float deltaTime = 0.1f;
    public Material lrMaterial = null;
    private float currentTime = 0;


    private void Awake() {
        trajPoints.Add(transform.position);
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 1;
        lr.SetPositions(trajPoints.ToArray());
        lr.SetWidth(0.01f, 0.01f);
        if (lrMaterial != null) lr.material = lrMaterial;
    }

    // Update is called once per frame
    private void Update() {
        currentTime += Time.deltaTime;
        if (currentTime >= deltaTime) {
            currentTime = 0f;
            trajPoints.Add(transform.position);
            lr.positionCount = trajPoints.Count;
            for(int i = 0; i < trajPoints.Count; i++) {
                lr.SetPosition(i,trajPoints[i]);
            }
        }
    }
}
