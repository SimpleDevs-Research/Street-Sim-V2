using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.AI;

using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

using DataStructures.ViliWonka.KDTree;

using RVO;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PedestrianController : MonoBehaviour
{
    Animator m_animator;
    PedestrianMover m_pedestrianMover;

    [System.Serializable]
    public struct PedPersonality
    {
        public float riskAversion;
        public float dirtinessAversion;
        public float crowdednessAversion;
        public float distanceAversion;
        public float litterInclination;
    }

    public PedPersonality m_personality;


    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_pedestrianMover = GetComponent<PedestrianMover>();

        m_personality = new PedPersonality();
        m_personality.riskAversion = UnityEngine.Random.Range(0f, 1f);
        m_personality.dirtinessAversion = UnityEngine.Random.Range(0f, 1f);
        m_personality.crowdednessAversion = UnityEngine.Random.Range(0f, 1f);
        m_personality.distanceAversion = UnityEngine.Random.Range(0f, 1f);
        m_personality.litterInclination = UnityEngine.Random.Range(0f, 0f);
    }
    private void LateUpdate()
    {
        AnimatePedestrian();
    }

    //Procedurally update the animation on the pedestrian according to its current motion
    private void AnimatePedestrian()
    {
        float forward = m_pedestrianMover.m_currentVelocity.magnitude;
        if (m_animator == null) return;
        m_animator.SetFloat("Forward", forward * 0.3f, 0.1f, Time.deltaTime);
    }
}
