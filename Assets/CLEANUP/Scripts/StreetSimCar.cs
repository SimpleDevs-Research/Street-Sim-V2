using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using Helpers;
using Random = UnityEngine.Random;

public class StreetSimCar : MonoBehaviour
{
    [Header("=== REFERENCES ===")]
    public ExperimentID id;
    private Transform currentXRCamera = null;
    public Transform frontOfCar;
    public TrafficSignal trafficSignal;
    public Transform startTarget, middleTarget, endTarget;
    public TestTurret testTurret = null;
    [SerializeField] private Velocity Velocity;
    [SerializeField] private TrialPositionNotifier Notifier;
    [SerializeField] private Transform[] wheels;
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private AudioSource m_honkSource;

    [Header("=== CAR SETTINGS ===")]
    public float spaceMaximal = 6f;
    [SerializeField] private float accelerationMax = 10f, accelerationPref = 5f;
    [SerializeField] private AnimationCurve m_maxSpeedWeight;
    [SerializeField] private bool m_addToHistory = false;

    [Header("=== OUTCOMES (READ-ONLY) ===")]
    private Transform currentTarget;
    private RaycastHit carRaycastHit;
    private bool m_hitMid = false;
    private float originalSpeedTargeted;
    private float m_distanceTraveled = 0f;
    [SerializeField] private bool _is_active = false;
    public bool is_active => is_active;
    [SerializeField] private float timePref = 1f;
    [SerializeField] private float speedTargeted = 10f;
    [SerializeField] private float accelerationExpected = 0f;
    [SerializeField] private float spaceMinimal, spaceOptimal;
    [SerializeField] private float timeAgentInFront = -1f;
    [SerializeField] private float durationAgentInFront = 0f;
    [SerializeField] private float delayUntilHonk = 5f;
    [SerializeField] private Vector3 positionDiff = Vector3.zero;
    [SerializeField] private bool foundInFront = false;
    [SerializeField] private StreetSimCar followingCar = null;
    [SerializeField] private bool agentInFront = false;
    [SerializeField] private bool passedTraffic = false;

    private void Awake() {
        /* =================== UNKNOWN ================ */
        if (id == null) id = gameObject.GetComponent<ExperimentID>();
        if (Notifier == null) Notifier = GetComponent<TrialPositionNotifier>();

        /* ====== NECESSARY ====== */
        Velocity = GetComponent<Velocity>();
        if (testTurret == null) testTurret = GetComponent<TestTurret>();
    }

    public void Initialize(Transform xrCamera, bool addToHistory) {

        // This is called by the `StreetSimCarManager` script. It toggles this car into active state.

        // Step 1: set the position and rotation of this car to match the start.
        transform.position = startTarget.position;
        transform.rotation = startTarget.rotation;

        // Step 2: Set the XR camera reference
        currentXRCamera = xrCamera;

        // Step 3: Set the end target
        currentTarget = endTarget;
        
        // Step 4: Turn on components
        m_audioSource.enabled = true;
        m_honkSource.enabled = true;
        testTurret.enabled = true;
        if (Notifier != null) Notifier.enabled = true;

        // Step 5: Let the car know they haven't crossed the midpoint yet
        m_hitMid = false;

        // Step 6: Now we set all the variou settings of this car, based on randomizers and the like.
        Velocity.manualSpeed = 0f;
        speedTargeted = 5f + (CalculateMaxSpeed()/10f);
        originalSpeedTargeted = speedTargeted;
        accelerationExpected = 0f;
        spaceMinimal = UnityEngine.Random.Range(0.25f,0.75f);
        timePref = UnityEngine.Random.Range(0.25f,0.75f);
        m_distanceTraveled = 0f;
        timeAgentInFront = -1f;
        delayUntilHonk = Random.Range(3f,7f);
        passedTraffic = false;

        // Step 7: To wrap up, we set the active state of this car.
        _is_active = true;
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
        Velocity.manualSpeed = 0f;
        if (Notifier != null) Notifier.enabled = false;
    }

    private void Update() {
        // Don't do anything if we're idle
        if (!_is_active) return;

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
        float speedDiff = (foundInFront) 
            ? Velocity.manualSpeed - carRaycastHit.transform.GetComponent<Velocity>().manualSpeed 
            : (!passedTraffic)
                ? (trafficSignal.status == TrafficSignal.TrafficSignalStatus.Stop) 
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
        if (!_is_active) return;

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

    public void SetActiveState(bool set_to) {
        _is_active = set_to;
    }
}
