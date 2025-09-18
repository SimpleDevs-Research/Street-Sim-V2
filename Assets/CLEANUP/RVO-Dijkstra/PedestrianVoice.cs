using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianVoice : MonoBehaviour
{
    public float m_tooCloseRadius;
    public AudioClip m_tooCloseVoice;
    public bool spoken = false;
    public void Update()
    {
        if(!spoken && Vector3.Distance(transform.position, PlayerTracker.Instance.transform.position) < m_tooCloseRadius)
        {
            GetComponent<AudioSource>().clip = m_tooCloseVoice;
            GetComponent<AudioSource>().Play();
            spoken = true;
        }
    }
}
