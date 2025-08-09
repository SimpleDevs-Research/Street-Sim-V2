using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ReplayWriter : Writer
{



    [Header("=== Outputs ===")]
    public CSVWriter positionWriter;


    void Awake()
    {
        current = this;
    }

    void Start()
    {
        
        positionWriter.Initialize();

    }

    public override void UpdatePosition(float t, int frame, string _name, int _guid, Vector3 p, Vector3 f)
    {
        if (positionWriter.is_active)
        {
            positionWriter.AddPayload(t);
            positionWriter.AddPayload(frame);
            positionWriter.AddPayload(_name);
            positionWriter.AddPayload(_guid);
            positionWriter.AddPayload(p);
            positionWriter.AddPayload(f);
            positionWriter.WriteLine(true);
        }
    }


}
