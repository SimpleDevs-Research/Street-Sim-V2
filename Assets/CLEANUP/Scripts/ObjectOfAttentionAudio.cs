using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectOfAttentionAudio : ObjectOfAttention
{
    // Start is called before the first frame update
    float currentVolume;
    public bool useMicrophone;
    MicrophoneSoundListener microphone;
    public float minimumSound;
    public float attentionPriorityMultiplier;
    void Start()
    {
        maxDistance = GetComponent<SphereCollider>().radius;
        if (useMicrophone) microphone = GetComponent<MicrophoneSoundListener>();
    }

    public override float GetAttentionPriority(Transform theTransform)
    {
        if(useMicrophone)
        {
            currentVolume = Mathf.Max(0f, microphone.CurrentLoudness);
        }
        float L = Mathf.Max(0f, currentVolume);
        Vector3 src = theTransform.position;
        float r = Vector3.Distance(transform.position, src);
        float r0 = maxDistance;

        float thePriority = attentionPriority;
        thePriority = L * (r0 * r0) / Mathf.Max(r * r, 1e-4f);

        dbg_L = L;
        dbg_r = r;

        if(thePriority < minimumSound)
        {
            return 0;
        }
        return thePriority * attentionPriorityMultiplier;
    }

    [Header("Per-agent HUD (optional)")]
    public bool debugHUD = false;
    public bool hudAutoStack = true;
    public Vector2 hudPosition = new Vector2(20, 20);

    float dbg_L, dbg_r;
    string dbg_state = "";

    void OnGUI()
    {
        if (!useMicrophone) return;

        float stackY = (0f);
        float x = hudPosition.x, y = hudPosition.y + stackY;
        float W = 260f, H = 140f;
        GUI.Box(new Rect(x, y, W, H), name + " Debug");

        GUI.Label(new Rect(x + 10, y + 24, W - 20, 18), $"State: {dbg_state}");
        GUI.Label(new Rect(x + 10, y + 42, W - 20, 18), $"L={dbg_L:F3}  r={dbg_r:F2}");

        float barX = x + 10, barY = y + 64, barW = W - 20, barH = 14;
        float barY2 = y + 100;
        GUI.color = Color.white;
        GUI.Label(new Rect(barX, barY2 - 16, 140, 30), "Raw Loudness (L)");
        GUI.Box(new Rect(barX, barY2, barW, barH), GUIContent.none);
        float Lw = Mathf.Clamp01(dbg_L * 12f) * (barW - 2);
        GUI.color = new Color(0.9f, 0.9f, 0.2f);
        GUI.DrawTexture(new Rect(barX + 1, barY2 + 1, Lw, barH - 2), Texture2D.whiteTexture);
    }
}
