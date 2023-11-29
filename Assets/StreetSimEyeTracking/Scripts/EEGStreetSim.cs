using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class EEGStreetSim : MonoBehaviour
{

    public class StreetSimEvent {
        public long unix_ts;
        public string event_type;
        public string title;
        public string description;
        public float x, y, z;
    }

    public static EEGStreetSim ESS;

    public string name;
    public Transform xrCamera;
    public EyeTrackingRay leftEyeTracker, rightEyeTracker;
    public LayerMask positionRaycastLayerMask;

    [SerializeField] private string filePath;
    [SerializeField] private float startTime;
    private StreamWriter eventWriter;
    private IEnumerator eventCoroutine = null;

    void OnEnable() {
        string fname = name + "-" + System.DateTime.Now.ToString("HH-mm-ss") + ".csv";
        filePath = Path.Combine(Application.persistentDataPath, fname);
        startTime = GetUnixTime();
    }

    void Awake() {
        ESS = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        eventWriter = new StreamWriter(new FileStream(filePath, FileMode.Create), Encoding.UTF8);
        // Header Line
        eventWriter.WriteLine("unix_ts,event_type,title,description,x,y,z");
        // First Entry: Start
        eventWriter.WriteLine(EventLine(startTime,"Simulation", Vector3.zero, "Simulation Start"));
        // Start the event coroutine
        eventCoroutine = EventCoroutine();
        StartCoroutine(eventCoroutine);
    }

    private IEnumerator EventCoroutine() {
        while(true) {
            // Calculate the current time
            float currentTime = GetUnixTime();
            // Check what's underneath the player currently
            RaycastHit hit;
            string belowTargetName = "Unknown";
            if (Physics.Raycast(xrCamera.position, -Vector3.up, out hit, 5f, positionRaycastLayerMask)) {
                belowTargetName = hit.transform.gameObject.name;
            }
            // Create a record for the player's current position
            eventWriter.WriteLine(EventLine(currentTime,"Player",xrCamera.position,"position",belowTargetName));
            // Create a record for the player's current orientation
            eventWriter.WriteLine(EventLine(currentTime,"Player",xrCamera.rotation,"orientation"));
            // Create a record for each eye
            if (leftEyeTracker != null && leftEyeTracker.rayHit) {
                eventWriter.WriteLine(EventLine(currentTime,"Global Eye Tracking", leftEyeTracker.rayTargetPosition, "Left", leftEyeTracker.rayTargetName));
                eventWriter.WriteLine(EventLine(currentTime,"Relative Eye Tracking", leftEyeTracker.rayTargetRelPosition, "Left", leftEyeTracker.rayTargetName));
            }
            if (rightEyeTracker != null && rightEyeTracker.rayHit) {
                eventWriter.WriteLine(EventLine(currentTime,"Global Eye Tracking", rightEyeTracker.rayTargetPosition, "Right", rightEyeTracker.rayTargetName));
                eventWriter.WriteLine(EventLine(currentTime,"Relative Eye Tracking", rightEyeTracker.rayTargetRelPosition, "Right", rightEyeTracker.rayTargetName));
            }
            // Yield return for the next event
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void WriteLine(string event_type, Vector3 xyz, string title="", string description="") {
        // Only continue if the event writer is not null
        if (eventWriter == null) return;
        // Calculate the current time
        float currentTime = GetUnixTime();
        // Write to the event writer
        eventWriter.WriteLine(EventLine(currentTime, event_type, xyz, title, description));
    }

    void OnDisable() {
        // Write the final line
        float endTime = GetUnixTime();
        eventWriter.WriteLine(EventLine(endTime, "Simulation", Vector3.zero, "Simulation End"));
        // Close and flush the writer
        eventWriter.Flush();
        eventWriter.Close();
        // End the coroutine
        StopCoroutine(eventCoroutine);
    }

    public static float GetUnixTime() {
        DateTime currentTime = DateTime.UtcNow;
        return ((float)((DateTimeOffset)currentTime).ToUnixTimeSeconds());
    }

    public static string EventLine(float unix_ts, string event_type, Vector3 xyz, string title="", string description="") {
        return $"{unix_ts},{event_type},{title},{description},{xyz.x},{xyz.y},{xyz.z}";
    }

    public static string EventLine(float unix_ts, string event_type, Quaternion q, string title="", string description="") {
        Vector3 xyz = q.eulerAngles;
        return $"{unix_ts.ToString("0.000")},{event_type},{title},{description},{xyz.x},{xyz.y},{xyz.z}";
    }
}
