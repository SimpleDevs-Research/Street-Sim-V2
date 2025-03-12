using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class GazeTrackRecorder : MonoBehaviour
{
    [Header("=== Writing Settings ===")]
    public bool activateOnStart = true;
    public CSVWriter writer;
    [Space]
    public float startTime = 0f;
    public float incrementTime = 1/60f;

    // =======================
    [Header("=== References ===")]
    public Transform headRef;
    public CombinedEyeTracker combinedEyeTracker;
    [Space]
    public Camera screenCamera;

    // =======================
    private IEnumerator updateCoroutine;

    

    private void Start() {
        if (activateOnStart) Activate();
    }

    public void Activate() {
        if (writer.is_active) {
            Debug.LogError("Cannot re-activate Gaze Track Recorder: writer is already active");
            return;
        }

        // Wait until writer is active
        if (writer.Initialize()) {
            updateCoroutine = RecordEyes();
            StartCoroutine(updateCoroutine);
        }
    }

    public void Deactivate() {
        StopCoroutine(updateCoroutine);
        // Add final line
        writer.AddPayload(GetCurrentTime());
        writer.AddPayload("Deactivate");
        writer.AddPayload("");
        writer.AddPayload("");
        writer.AddPayload("");
        writer.AddPayload("");
        writer.WriteLine(true);
        // Disable writer
        writer.Disable();
    }

    private IEnumerator RecordEyes() {
        // Initialize start time
        startTime = Time.time;

        // Add a single row to represent the start of the recording.
        writer.AddPayload(GetCurrentTime());
        writer.AddPayload("Activation");
        writer.AddPayload("");
        writer.AddPayload("");
        writer.AddPayload("");
        writer.AddPayload("");
        writer.WriteLine(true);

        // Initialize some variables
        Vector3 screenPos;
        string eventLabel, targetName;

        // Initialize wait for seconds
        WaitForSeconds timeDelay = new WaitForSeconds(incrementTime);

        // Initialize loop
        while(true) {
            
            // Get event
            eventLabel = "";
            screenPos = Vector3.zero;
            targetName = "";

            if (combinedEyeTracker.rayHit) {
                Vector3 worldPos = combinedEyeTracker.rayTargetPosition;
                // Check its screen position relative to the current camera
                screenPos = screenCamera.WorldToScreenPoint(worldPos);
                targetName = combinedEyeTracker.rayTargetName;
                eventLabel = "Eye Hit";
            }
            // Save to write
            writer.AddPayload(GetCurrentTime());
            writer.AddPayload(eventLabel);
            writer.AddPayload(screenPos);
            writer.AddPayload(targetName);
            writer.WriteLine(true);

            yield return timeDelay;
        }

        yield return null;
    }

    public float GetCurrentTime() {
        return Time.time - startTime;
    }

    void OnDestroy() {
        if (writer.is_active) Deactivate();
    }
}
