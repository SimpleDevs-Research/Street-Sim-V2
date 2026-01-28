using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianHeadWriter : MonoBehaviour
{

    public PedestrianController parent_agent;

    private void Update()
    {
        if (PedestrianWriter.current != null) PedestrianWriter.current.AddPedestrian(Time.frameCount, Time.time, $"{parent_agent.agent_label}_Head", this.transform);
    }
}
