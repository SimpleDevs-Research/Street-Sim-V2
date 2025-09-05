using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class GreetDebugOverlay : MonoBehaviour
{
    [Header("UI")]
    public bool show = true;
    public KeyCode toggleKey = KeyCode.F1;
    public Vector2 anchor = new Vector2(12, 12);
    public Vector2 size = new Vector2(520, 300);

    [Header("Table")]
    public bool sortByDistanceAsc = true;
    [Tooltip("How often to rescan the scene for agents (seconds).")]
    public float refreshEvery = 0.5f;

    private Vector2 _scroll;
    private float _nextScan;
    private readonly List<SimpleGreetController> _agents = new List<SimpleGreetController>();

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) show = !show;

        if (Time.time >= _nextScan)
        {
            _nextScan = Time.time + refreshEvery;
            _agents.Clear();
#if UNITY_2023_1_OR_NEWER
            _agents.AddRange(UnityEngine.Object.FindObjectsByType<SimpleGreetController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            ));
#else
            _agents.AddRange(FindObjectsOfType<SimpleGreetController>(true));
#endif
        }
    }

    void OnGUI()
    {
        if (!show) return;

        var rect = new Rect(anchor.x, anchor.y, size.x, size.y);
        GUI.Box(rect, "Greet Overlay  (F1 toggle)");
        var inner = new Rect(rect.x + 8, rect.y + 28, rect.width - 16, rect.height - 36);

        GUILayout.BeginArea(inner);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(sortByDistanceAsc ? "▲ Dist" : "▼ Dist", GUILayout.Width(70)))
            sortByDistanceAsc = !sortByDistanceAsc;
        GUILayout.Label("Name", GUILayout.Width(140));
        GUILayout.Label("State", GUILayout.Width(110));
        GUILayout.Label("r", GUILayout.Width(60));
        GUILayout.Label("L", GUILayout.Width(60));
        GUILayout.Label("Eff", GUILayout.Width(60));
        GUILayout.EndHorizontal();

        _scroll = GUILayout.BeginScrollView(_scroll);
        IEnumerable<SimpleGreetController> list = _agents.Where(a => a != null);
        list = sortByDistanceAsc ? list.OrderBy(a => a.DebugDistance) : list.OrderByDescending(a => a.DebugDistance);

        foreach (var a in list)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{a.DebugDistance,6:0.00}", GUILayout.Width(70));
            GUILayout.Label(a.name, GUILayout.Width(140));
            GUILayout.Label(a.DebugState, GUILayout.Width(110));
            GUILayout.Label($"{a.DebugDistance:0.00}", GUILayout.Width(60));
            GUILayout.Label($"{a.DebugRawLoudness:0.000}", GUILayout.Width(60));

            // Eff 条形
            float eff = a.DebugEffective;
            Rect r = GUILayoutUtility.GetRect(60, 16);
            GUI.Box(r, GUIContent.none);
            var prev = GUI.color;
            GUI.color = eff >= a.threshold ? new Color(0.2f, 0.9f, 0.2f) : new Color(0.2f, 0.6f, 1f);
            GUI.DrawTexture(new Rect(r.x + 1, r.y + 1, Mathf.Clamp01(eff) * (r.width - 2), r.height - 2), Texture2D.whiteTexture);
            GUI.color = prev;

            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
