using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class SimpleGreetController : MonoBehaviour
{
    [Header("Inputs")]
    public MicrophoneSoundListener microphone;

    [Header("Targeting")]
    public Transform lookTarget;
    public Transform soundSource;

    [Header("Proximity (optional)")]
    public ClapProximityZone proximityZone;
    public bool requireInsideZone = true;
    public float triggerRadius = 4f;
    public bool autoMatchRefDistance = true;
    [Range(0.1f, 1.5f)] public float refDistanceFactor = 0.9f;

    [Header("Threshold & Timing")]
    [Range(0f, 1f)] public float threshold = 0.06f;
    public float minInterval = 1.2f;
    public float holdSeconds = 0.6f;
    public float resumeDelay = 0.2f;

    [Header("Turning")]
    public float turnSpeed = 10f;

    [Header("Obstacle Proxy (optional)")]
    public GameObject obstacleProxy;
    public string obstacleProxyChildName = "ObstacleProxy";
    public bool useObstacleProxyDuringGreet = false;

    [Header("Pose control while greeting")]
    public bool forceIdleOnGreet = true;
    public string idleStateName = "Idle";
    public float idleCrossfade = 0.1f;
    public bool lockAgentRotationDuringGreet = true;

    [Header("Per-agent HUD (optional)")]
    public bool debugHUD = false;
    public bool hudAutoStack = true;
    public Vector2 hudPosition = new Vector2(20, 20);

    NavMeshAgent _agent;
    Animator _anim;
    AudioSource _audio;
    WanderAgent _wander;
    float _lastTs = -999f;
    bool _busy = false;

    static int sHudAlloc = 0;
    int hudOrder = 0;

    float dbg_L, dbg_r, dbg_r0, dbg_effective;
    string dbg_state = "";
    public bool IsBusy => _busy;
    public float DebugRawLoudness => dbg_L;
    public float DebugDistance => dbg_r;
    public float DebugRefDist => dbg_r0;
    public float DebugEffective => dbg_effective;
    public string DebugState => dbg_state;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        _wander = GetComponent<WanderAgent>();
    }
    void OnEnable() { if (hudAutoStack) hudOrder = sHudAlloc++; }
    void Start()
    {
        if (!obstacleProxy && !string.IsNullOrEmpty(obstacleProxyChildName))
        {
            var t = transform.Find(obstacleProxyChildName);
            if (t) obstacleProxy = t.gameObject;
        }
        if (obstacleProxy) obstacleProxy.SetActive(false);
    }

    void Update()
    {
        float L = microphone ? Mathf.Max(0f, microphone.CurrentLoudness) : 0f;
        Vector3 src = GetSoundSourcePosition();
        float r = Vector3.Distance(transform.position, src);

        bool inside = IsInside(r);
        float r0 = ComputeRefDistance();
        float effective = inside ? L * (r0 * r0) / Mathf.Max(r * r, 1e-4f) : 0f;

        bool readyByInterval = (Time.time - _lastTs) >= minInterval;
        bool shouldTrigger = effective >= threshold && readyByInterval && !_busy;

        if (_anim && _agent && !_busy)
        {
            float speed = (_agent.enabled && _agent.isOnNavMesh) ? _agent.velocity.magnitude : 0f;
            _anim.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
        }

        if (shouldTrigger) StartCoroutine(DoGreet(src));

        dbg_L = L; dbg_r = r; dbg_r0 = r0; dbg_effective = effective;
        if (_busy) dbg_state = "Busy";
        else if (!readyByInterval) dbg_state = $"Cooldown {(minInterval - (Time.time - _lastTs)):F1}s";
        else if (!inside) dbg_state = "Outside zone";
        else if (effective < threshold) dbg_state = "Below threshold";
        else dbg_state = "Ready";
    }

    public void TriggerOnce()
    {
        if (_busy) return;
        StartCoroutine(DoGreet(GetSoundSourcePosition()));
    }

    IEnumerator DoGreet(Vector3 targetPos)
    {
        _busy = true;
        _lastTs = Time.time;

        if (_wander) _wander.enabled = false;

        bool prevUpdateRot = _agent ? _agent.updateRotation : true;
        if (_agent && _agent.enabled)
        {
            if (lockAgentRotationDuringGreet) _agent.updateRotation = false;
            _agent.isStopped = true;
            _agent.ResetPath();
        }

        if (useObstacleProxyDuringGreet && obstacleProxy) obstacleProxy.SetActive(true);

        if (forceIdleOnGreet && _anim && !string.IsNullOrEmpty(idleStateName))
            _anim.CrossFadeInFixedTime(idleStateName, Mathf.Max(0.01f, idleCrossfade));

        float t = 0f;
        while (t < holdSeconds)
        {
            t += Time.deltaTime;

            if (_anim) _anim.SetFloat("Speed", 0f);
            if (_agent && _agent.enabled) _agent.isStopped = true;

            Vector3 dir = (new Vector3(targetPos.x, transform.position.y, targetPos.z) - transform.position);
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime * 60f);
            }
            yield return null;
        }

        if (_audio && _audio.clip)
        {
            _audio.Play();
            yield return new WaitForSeconds(_audio.clip.length);
        }

        yield return new WaitForSeconds(resumeDelay);

        if (useObstacleProxyDuringGreet && obstacleProxy) obstacleProxy.SetActive(false);

        if (_agent && _agent.enabled)
        {
            _agent.isStopped = false;
            _agent.updateRotation = prevUpdateRot;
        }
        if (_wander) _wander.enabled = true;

        _busy = false;
    }

    Vector3 GetSoundSourcePosition()
    {
        if (soundSource) return soundSource.position;
        if (lookTarget) return lookTarget.position;
        if (Camera.main) return Camera.main.transform.position;
        return transform.position + transform.forward * 2f;
    }

    bool IsInside(float r)
    {
        if (proximityZone)
        {
            if (proximityZone.avatar) return proximityZone.avatarInside;
            return proximityZone.ContainsPosition(transform.position);
        }
        if (!requireInsideZone) return true;
        return r <= triggerRadius;
    }

    float ComputeRefDistance()
    {
        if (!autoMatchRefDistance) return triggerRadius;
        if (proximityZone) return Mathf.Max(0.01f, proximityZone.WorldRadius * refDistanceFactor);
        return Mathf.Max(0.01f, triggerRadius * refDistanceFactor);
    }

    void OnGUI()
    {
        if (!debugHUD) return;

        float stackY = (hudAutoStack ? hudOrder * 150f : 0f);
        float x = hudPosition.x, y = hudPosition.y + stackY;
        float W = 260f, H = 140f;
        GUI.Box(new Rect(x, y, W, H), name + " Debug");

        GUI.Label(new Rect(x + 10, y + 24, W - 20, 18), $"State: {dbg_state}");
        GUI.Label(new Rect(x + 10, y + 42, W - 20, 18), $"L={dbg_L:F3}  r={dbg_r:F2}  r0={dbg_r0:F2}  T={threshold:F2}");

        float barX = x + 10, barY = y + 64, barW = W - 20, barH = 14;
        GUI.Label(new Rect(barX, barY - 16, 120, 14), "Effective");
        GUI.Box(new Rect(barX, barY, barW, barH), GUIContent.none);
        float effW = Mathf.Clamp01(dbg_effective) * (barW - 2);
        var prev = GUI.color;
        GUI.color = (dbg_effective >= threshold) ? new Color(0.2f, 0.9f, 0.2f) : new Color(0.2f, 0.6f, 1f);
        GUI.DrawTexture(new Rect(barX + 1, barY + 1, effW, barH - 2), Texture2D.whiteTexture);

        GUI.color = Color.red;
        float tX = barX + Mathf.Clamp01(threshold) * barW;
        GUI.DrawTexture(new Rect(tX, barY, 2, barH), Texture2D.whiteTexture);

        float barY2 = y + 100;
        GUI.color = prev;
        GUI.Label(new Rect(barX, barY2 - 16, 140, 14), "Raw Loudness (L)");
        GUI.Box(new Rect(barX, barY2, barW, barH), GUIContent.none);
        float Lw = Mathf.Clamp01(dbg_L * 12f) * (barW - 2);
        GUI.color = new Color(0.9f, 0.9f, 0.2f);
        GUI.DrawTexture(new Rect(barX + 1, barY2 + 1, Lw, barH - 2), Texture2D.whiteTexture);
        GUI.color = prev;
    }
}
