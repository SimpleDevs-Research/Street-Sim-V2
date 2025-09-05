using UnityEngine;

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

    void Start()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("[Microphone] No device!");
            enabled = false; return;
        }
        if (string.IsNullOrEmpty(microphoneName))
            microphoneName = Microphone.devices[0];

        samples = new float[sampleWindow];
        microphoneClip = Microphone.Start(microphoneName, true, 1, sampleRate);
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

    void OnDestroy()
    {
        if (!string.IsNullOrEmpty(microphoneName))
            Microphone.End(microphoneName);
    }
}
