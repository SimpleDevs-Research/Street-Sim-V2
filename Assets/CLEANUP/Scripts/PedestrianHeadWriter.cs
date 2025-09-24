using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianHeadWriter : MonoBehaviour
{

    private void Update()
    {
        if (PedestrianWriter.current != null) PedestrianWriter.current.AddPedestrian(Time.frameCount, Time.time, "PedestrianHead", this.transform);
    }
}
