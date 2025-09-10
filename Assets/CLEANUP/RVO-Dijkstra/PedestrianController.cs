using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.AI;
using Unity.Jobs;
using Unity.Burst;
using DataStructures.ViliWonka.KDTree;
using RVO;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PedestrianController : Entity
{
    Animator m_animator;
    PedestrianMover m_pedestrianMover;

    [SerializeField] private RouteNode m_routeDestination;
    [SerializeField] private RouteNode m_routeStart;
    [SerializeField] public Vector3 m_segmentDestination;
    [SerializeField] private List<RouteNode> m_route;
    [SerializeField] private int m_routeNodeIndex = 1;

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

        m_segmentDestination = transform.position;
    }
    private void LateUpdate()
    {
        AnimatePedestrian();
    }

    //Check a pedestrians position on their global route, find the current segment destination
    public bool QueryGlobalRoute()
    {
        if (m_route.Count <= 1)
        {
            GetComponent<PedestrianMover>().m_optimalVelocity = Vector3.zero;
            PedestrianManager.Instance.PedestrianAtEnd(this);
            return false;

        }

        float acceptableRadius = m_route[1].acceptableRadius;
        // End early if we're close enough to our final destination
        if (Vector3.Distance(m_segmentDestination, transform.position) <= acceptableRadius)
        {

            List<RouteNode> route = RouteManager.instance.getRoute(m_route[1], m_routeDestination, GetComponent<PedestrianController>().m_personality);
            SetRoute(route);

            //If we've reached the final part of the route, end
            if (m_route.Count <= 1)
            {
                GetComponent<PedestrianMover>().m_optimalVelocity = Vector3.zero;
                PedestrianManager.Instance.PedestrianAtEnd(this);
                return false;
            }
            else
            {
                SetSegmentDestination(route[1].transform.position
                    + new Vector3(UnityEngine.Random.Range(-acceptableRadius, acceptableRadius),
                                    0,
                                    UnityEngine.Random.Range(-acceptableRadius, acceptableRadius)));
            }
        }
        return true;

        //SetSegmentDestination(PlayerTracker.Instance.transform.position);
        //return true;
    }

    public void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("RerouteTrigger"))
        {
            List<RouteNode> route = RouteManager.instance.getRoute(m_route[0], m_routeDestination, GetComponent<PedestrianController>().m_personality);
            float acceptableRadius = m_route[0].acceptableRadius;

            SetRoute(route);
            SetSegmentDestination(route[1].transform.position
                    + new Vector3(UnityEngine.Random.Range(-acceptableRadius, acceptableRadius),
                                    0,
                                    UnityEngine.Random.Range(-acceptableRadius, acceptableRadius)));
        }
    }

    //Procedurally update the animation on the pedestrian according to its current motion
    private void AnimatePedestrian()
    {
        float forward = m_pedestrianMover.m_currentVelocity.magnitude;
        if (m_animator == null) return;
        m_animator.SetFloat("Forward", forward * 0.3f, 0.1f, Time.deltaTime);
    }
    public void SetSegmentDestination(Vector3 d)
    {
        m_segmentDestination = d;
    }
    public void SetRouteDestination(RouteNode routeNode)
    {
        m_routeDestination = routeNode;
    }
    public void SetRouteStart(RouteNode routeNode)
    {
        m_routeStart = routeNode;
    }
    public void SetRoute(List<RouteNode> route)
    {
        m_route = route;

        /*String pathString = route[0].gameObject.name;
        for (int i = 1; i < route.Count; i++)
        {
            pathString += " -> " + route[i].gameObject.name;
        }
        print(pathString);*/
    }
}
