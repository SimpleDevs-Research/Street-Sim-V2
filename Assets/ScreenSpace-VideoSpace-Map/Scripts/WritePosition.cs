using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WritePosition : MonoBehaviour
{
    public Camera center_eye_ref;
    public Camera left_eye_ref;
    public Camera right_eye_ref;
    public CSVWriter writer;
    public bool treat_as_pedestrian;

    // Frame capture
    public bool capture_frames = true;
    public int targetFPS = 5;
    public string outputDir = "frames";

    private float interval;
    private float timer = 0f;
    private int captureCount = 0;

    void Start()
    {
        if (capture_frames)
        {
            interval = 1f / targetFPS;
            System.IO.Directory.CreateDirectory(outputDir);
        }
    }

    void Update()
    {
        if (capture_frames)
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                ScreenCapture.CaptureScreenshot(
                    $"{outputDir}/frame_{captureCount:D4}.png"
                );
                captureCount++;
                timer = 0f;
            }
        }

        if (PedestrianWriter.current != null) PedestrianWriter.current.AddPedestrian(Time.frameCount, Time.time, gameObject.name, this.transform);
    }

    void OnDestroy()
    {
        writer.Disable();
    }
}