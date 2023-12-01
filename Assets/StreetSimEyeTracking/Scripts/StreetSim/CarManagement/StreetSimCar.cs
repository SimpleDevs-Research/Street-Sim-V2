using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using Helpers;
using Random = UnityEngine.Random;

public class StreetSimCar : MonoBehaviour
{
    public enum StreetSimCarStatus {
        Idle,
        Active,
    }
    public ExperimentID id;
    private Transform currentXRCamera = null;
    public Transform frontOfCar, backOfCar;
    [SerializeField] private RemoteCollider frontCollider;
    public TrafficSignal trafficSignal;
    public Transform startTarget, middleTarget, endTarget;
    public RemoteCollider agentDetector;
    public TestTurret testTurret = null;
    [SerializeField] private Collider[] gazeColliders;
    [SerializeField] private Velocity Velocity;

    [SerializeField] private float m_lengthOfCar = 0f;
    [SerializeField] private float maxSpeed = 0.5f;
    private float m_originalMaxSpeed;
    [SerializeField] private bool shouldStop = false;
    public StreetSimCarStatus status = StreetSimCarStatus.Idle;
    private float smoothTime;
    private float currentTime = 0f;

    [SerializeField] private float acceleration = 5f, deceleration = 7.5f;
    private float m_originalDeceleration;

    private Transform currentTarget;
    private Vector3 prevTargetPos;

    [SerializeField] private Transform[] wheels;
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private AudioSource m_honkSource;

    private RaycastHit carRaycastHit;
    [SerializeField] private bool foundInFront = false;
    [SerializeField] private bool agentInFront = false;
    [SerializeField] private StreetSimCar followingCar = null;
    private bool m_hitMid = false;

    private Vector3 prevPos;
    public float speed = 0f;
    [SerializeField] private Vector3 positionDiff = Vector3.zero;
    [SerializeField] private Vector3 velocityDiff = Vector3.zero;
    [SerializeField] private float spaceMinimal = 0.5f, spaceOptimal, spaceMaximal = 6f;
    [SerializeField] private float accelerationMax = 10f, accelerationPref = 5f;
    [SerializeField] private float accelerationExpected = 0f;
    [SerializeField] private float speedTargeted = 10f;
    private float originalSpeedTargeted;
    [SerializeField] private float timePref = 1f;
    [SerializeField] private float delayUntilHonk = 5f;
    [SerializeField] private float timeAgentInFront = -1f;
    [SerializeField] private float durationAgentInFront = 0f;
    private float m_distanceTraveled = 0f;
    [SerializeField] private bool passedTraffic = false;

    [SerializeField] private AnimationCurve m_maxSpeedWeight;
    [SerializeField] private Vector2 minMaxViewAngles = new Vector2(80f,20f);

    [SerializeField] private bool m_addToHistory = false;

    private void Awake() {
        if (id == null) id = gameObject.GetComponent<ExperimentID>();
        m_lengthOfCar = GetComponent<BoxCollider>().size.z * transform.localScale.z;
        Velocity = GetComponent<Velocity>();
        if (testTurret == null) testTurret = GetComponent<TestTurret>();
        m_originalDeceleration = deceleration;
    }

    public void Initialize(Transform xrCamera, bool addToHistory) {
        transform.position = startTarget.position;
        transform.rotation = startTarget.rotation;
        
        foreach(Collider col in gazeColliders) col.enabled = true;

        currentXRCamera = xrCamera;
        currentTarget = endTarget;
        prevPos = transform.position;
        prevTargetPos = endTarget.position;
        
        m_audioSource.enabled = true;
        m_honkSource.enabled = true;
        testTurret.enabled = true;

        m_hitMid = false;

        //maxSpeed = UnityEngine.Random.Range(5f,15f);
        maxSpeed = 5f + (CalculateMaxSpeed()/10f);
        m_originalMaxSpeed = maxSpeed;

        Velocity.manualSpeed = 0f;
        speedTargeted = maxSpeed;
        originalSpeedTargeted = speedTargeted;
        accelerationExpected = 0f;
        spaceMinimal = UnityEngine.Random.Range(0.25f,0.75f);
        timePref = UnityEngine.Random.Range(0.25f,0.75f);
        m_distanceTraveled = 0f;
        timeAgentInFront = -1f;
        delayUntilHonk = Random.Range(3f,7f);
        passedTraffic = false;

        status = StreetSimCarStatus.Active;
        m_addToHistory = addToHistory;
    }

    private float CalculateMaxSpeed() {
        float[] indexes = new float[101];
        float[] weights = new float[101];
        float weightSum = 0f;
        for(int i = 0; i <= 100; i++) {
            float fI = (float)i / 100f;
            indexes[i] = fI;
            weights[i] = m_maxSpeedWeight.Evaluate(fI);
            weightSum += weights[i];
        }
        int index = 0;
        int lastIndex = 100;
        while(index < lastIndex) {
            if (Random.Range(0f, weightSum) < weights[index]) {
                return (float)index;
            }
            weightSum -= weights[index];
            index += 1;
        }
        return (float)index;
    }

    private void ReturnToIdle() {
        StreetSimCarManager.CM.SetCarToIdle(this);
        m_audioSource.enabled = false;
        m_honkSource.enabled = false;
        testTurret.SetObjects(new List<Transform>());
        testTurret.enabled = false;
        foreach(Collider col in gazeColliders) col.enabled = false;
        Velocity.manualSpeed = 0f;
    }

    /*
    private Vector3 SuperSmoothLerp(Vector3 x0, Vector3 y0, Vector3 yt, float t, float k) {
        Vector3 f = x0 - y0 + (yt - y0) / (k * t);
        return yt - (yt - y0) / (k*t) + f * Mathf.Exp(-k*t);
    }
    */

    private void Update() {
        // Don't do anything if we're idle
        if (status == StreetSimCarStatus.Idle) return;

        // update testTurrret with most recent list of active entities
        List<Transform> agentTargets = new List<Transform>();
        if (StreetSimAgentManager.AM != null) {
            List<StreetSimAgent> activeModels = StreetSimAgentManager.AM.GetActiveAgents();
            foreach(StreetSimAgent agent in activeModels) {
                agentTargets.Add(agent.transform);
            }
        }
        if (currentXRCamera != null) {
            agentTargets.Add(currentXRCamera);
        }
        testTurret.SetObjects(agentTargets);
        
        // Check if there's a car in front of us.
        //  foundInFront = global variable : boolean
        //  out carRaycastHit = global variable : RaycastHit
        foundInFront = Physics.Raycast(frontOfCar.position,frontOfCar.forward, out carRaycastHit, spaceMaximal, StreetSimCarManager.CM.carDetectionLayerMask);
        // also found in front if there is any targets found by testTurret
        followingCar = (foundInFront) 
            ? carRaycastHit.transform.GetComponent<StreetSimCar>()
            : null;
        agentInFront = testTurret.AnyInRange();
        if (trafficSignal.status != TrafficSignal.TrafficSignalStatus.Stop && Velocity.manualSpeed < 1f && (agentInFront || foundInFront)) {
            if (timeAgentInFront == -1) {
                timeAgentInFront = Time.time;
            }
            durationAgentInFront = Time.time - timeAgentInFront;
            if (durationAgentInFront >= delayUntilHonk) {
                m_honkSource.Play();
                timeAgentInFront = Time.time;
                delayUntilHonk = Random.Range(2f,5f);
            } 
        } else {
            timeAgentInFront = -1f;
            durationAgentInFront = 0f;
        }

        // Calculate position and velocity changes
        CalculateAcceleration();
        // Check how far we've moved
        CalculateDistanceFromStart();
        // After we do the calculation, we actually don't do anything else if we passed the midpoint
        if (m_hitMid) return;
        if (startTarget.position.x*transform.position.x<0f || Mathf.Abs(transform.position.x) <= 0.01f) {
            if (m_addToHistory) StreetSimCarManager.CM.AddCarMidToHistory(this,StreetSim.S.GetTimeFromStart(Time.time));
            m_hitMid = true;
        }
    }

    private void CalculateAcceleration() {
        passedTraffic = Vector3.Dot((middleTarget.position - frontOfCar.position).normalized, frontOfCar.forward) < 0f;
        float L = (!passedTraffic && (trafficSignal.status == TrafficSignal.TrafficSignalStatus.Stop || agentInFront) )
            ? 1f
            : 0f;
        float O = (foundInFront)
            ? 1f
            : 0f;
        float mSpeed = Mathf.Clamp(originalSpeedTargeted+originalSpeedTargeted*0.1f*(1f-O),0f,15f);
        L = (mSpeed <= 14f)
            ? L
            : 0f;

        positionDiff = (carRaycastHit.point-frontOfCar.position)*O + 
            (
                (middleTarget.position-frontOfCar.position)*L + 
                new Vector3(spaceOptimal+1f,0f,0f)*(1f-L)
            )*(1f-O);
        // The bottom SHOULD be how we do this...
        // float speedDiff = (speed-carRaycastHit.transform.GetComponent<StreetSimCar>().speed)*O + (speed*L)*(1f-O);
        float speedDiff = (foundInFront) 
            ? Velocity.manualSpeed - carRaycastHit.transform.GetComponent<Velocity>().manualSpeed 
//            : (!passedTraffic && trafficSignal.status != TrafficSignal.TrafficSignalStatus.Go && (speed < 14f || (speed >= 14f && positionDiff.magnitude < spaceMinimal)))
            : (!passedTraffic)
                ? (
                    trafficSignal.status == TrafficSignal.TrafficSignalStatus.Stop 
                    //|| agentDetector.numColliders > 0
                ) 
                    ? Velocity.manualSpeed
                    : 0f
                : 0f;

        spaceOptimal = (
            1f-
            (1f-L)
            *(1f-O)
            //*(1f-P)
        )*(spaceMinimal + Velocity.manualSpeed * timePref) + (Velocity.manualSpeed*speedDiff)/(2*Mathf.Pow(accelerationMax*accelerationPref,0.5f));
        accelerationExpected = accelerationMax * (
            1f - Mathf.Pow((Velocity.manualSpeed/mSpeed),4f) 
            - Mathf.Pow((spaceOptimal/positionDiff.magnitude),2f)
        );
    }
    private void CalculateDistanceFromStart() {
        m_distanceTraveled = Vector3.Distance(transform.position,startTarget.position);
    }

    private void FixedUpdate() {
        // don't do anything if we're idle
        if (status == StreetSimCarStatus.Idle) return;

        // We end out of the loop if we've reached our target and that target happens to be the same position as the endtarget
        if (Vector3.Distance(transform.position,endTarget.position) <= 0.01f || m_distanceTraveled >= 150f) {
            ReturnToIdle();
            return;
        }

        // We also pause if any values are missing
        if (trafficSignal == null || middleTarget == null || endTarget == null) return;
        
        UpdateSequence2();
    }

    private void UpdateSequence2() {
        // We know current acceleration `accelerationExpected`
        // We convert that to speed, then to position
        Velocity.manualSpeed += accelerationExpected * Time.fixedDeltaTime;
        Velocity.manualSpeed = Mathf.Max(Velocity.manualSpeed, 0f);
        transform.position = transform.position + transform.forward.normalized * Velocity.manualSpeed * Time.fixedDeltaTime; 
        // Spin our wheels, if we have any
        if (wheels.Length > 0) {
            foreach(Transform wheel in wheels) wheel.Rotate(Velocity.manualSpeed,0f,0f,Space.Self);
        }
    }

    public float GetCurrentSpeed() {
        return Velocity.manualSpeed;
    }
}
