using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReplaySet;

public class ReplayPositionNotifier : MonoBehaviour
{
    public int _guid;
    public string _name;

    private void Awake()
    {
        _name = this.gameObject.name;
        _guid = this.gameObject.GetInstanceID();
    }
    private void Start()
    {
        Replay.Instance.position_extraction_targets.Add(this);
    }
    private void OnDestroy()
    {
        Replay.Instance.position_extraction_targets.Remove(this);
    }
}
