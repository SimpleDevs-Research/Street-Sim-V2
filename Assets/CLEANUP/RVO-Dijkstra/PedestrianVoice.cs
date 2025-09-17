using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianVoice : MonoBehaviour
{
    public float m_tooCloseRadius;
    public AudioClip m_tooCloseVoice;

    public void Update()
    {
        if(Vector3.Distance(transform.position, PlayerTracker.Instance.transform.position) < m_tooCloseRadius && !GetComponent<AudioSource>().isPlaying)
        {
            GetComponent<AudioSource>().clip = m_tooCloseVoice;
            GetComponent<AudioSource>().Play();
        }
    }
}
