using UnityEngine;
using System.IO;


public class MicrophoneSoundListener : MonoBehaviour
{
    [Header("Microphone")]
    public string microphoneName = "";
    public int sampleRate = 44100;
    public int sampleWindow = 128;

    [Header("Smoothing")]
    [Range(1f, 50f)] public float smoothing = 10f;

    private AudioClip microphoneClip;
    private float[] samples;
    private float smoothedLoudness = 0f;

    public float CurrentLoudness => smoothedLoudness;

    [Header("Writing Output WAV")]
    public bool record_wav = true;
    public int max_recording_seconds = 600;
    public string fileName = "";
    public string dirName = "";
    public bool append_zero_to_filename = false;

    void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("[Microphone] No device!");
            enabled = false; return;
        }
        if (string.IsNullOrEmpty(microphoneName))
            microphoneName = Microphone.devices[0];
        print($"Microphone Listening to {microphoneName}");

        samples = new float[sampleWindow];
        microphoneClip = Microphone.Start(microphoneName, true, max_recording_seconds, sampleRate);
        while (Microphone.GetPosition(microphoneName) <= 0) { }
    }

    void Update()
    {
        if (microphoneClip == null) return;
        int pos = Microphone.GetPosition(microphoneName);
        if (pos < sampleWindow) return;

        microphoneClip.GetData(samples, pos - sampleWindow);

        float sum = 0f;
        for (int i = 0; i < samples.Length; i++) sum += samples[i] * samples[i];
        float rms = Mathf.Sqrt(sum / samples.Length);

        smoothedLoudness = Mathf.Lerp(smoothedLoudness, rms, Time.deltaTime * smoothing);
    }

    public void AddClickMarker(float amplitude = 0.9f) {
        if (microphoneClip == null) return;
        int pos = Microphone.GetPosition(microphoneName);
        int channels = microphoneClip.channels;
        // Convert to sample index in the clip’s data array
        int sampleIndex = pos * channels;
        float[] data = new float[microphoneClip.samples * channels];
        microphoneClip.GetData(data, 0);
        // Write an impulse (1–3 samples is enough)
        for (int c = 0; c < channels; c++) {
            int idx = sampleIndex + c;
            if (idx < data.Length)
                data[idx] = amplitude;  // impulse spike
        }
        microphoneClip.SetData(data, 0);
    }

    void OnApplicationPause()
    {
        if (!string.IsNullOrEmpty(microphoneName)) {
            
            int mposition = Microphone.GetPosition(microphoneName);
            Microphone.End(microphoneName);
            
            // Save as a wav file
            if (microphoneClip != null) {

                // Trim audio
                AudioClip trimmed_audioclip = WavUtility.TrimClip(microphoneClip, mposition);

                // Determine optimal directory name
                string dname = $"{Application.persistentDataPath}/{Helpers.SaveSystemMethods.GetCurrentDateTime()}";
                if (dirName != null && dirName.Length > 0) {
                    dname = $"{Application.persistentDataPath}/{dirName}";
                }

                // Determine optimal file name and resulting filepath
                string fname = (fileName != null && fileName.Length > 0) ? fileName : System.DateTime.Now.ToString("HH-mm-ss");
                string filePath = (append_zero_to_filename) ? Path.Combine(dname, fname+"_0.wav") : Path.Combine(dname, fname+".wav");
                int counter = 1;
                while(File.Exists(filePath)) {
                    Debug.Log("Path exists");
                    filePath = Path.Combine(dname, fname+$"_{counter}.wav");
                    counter++;
                }

                // Save the trimmed clip
                WavUtility.SaveWav(filePath, trimmed_audioclip);

            } else {
                Debug.Log("Microphone Clip is NULL - Cannot save audio recording");
            }
        } else {
            Debug.Log("No microphone name - cannot save audio recording");
        }
    }
}
