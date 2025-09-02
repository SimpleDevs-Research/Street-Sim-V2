using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReplaySet;

public class ReplayPositionNotifier : MonoBehaviour
{
    public int _guid;
    public string _name;
    Replay replay;

    private void Awake()
    {
        _name = this.gameObject.name;
        _guid = this.gameObject.GetInstanceID();
        replay = Replay.Instance;
    }
    private void Start()
    {
        if(replay != null)
            Replay.Instance.position_extraction_targets.Add(this);
    }
    private void OnDestroy()
    {
        if(replay != null)
            Replay.Instance.position_extraction_targets.Remove(this);
    }
}
