using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialController : MonoBehaviour
{
    [System.Serializable]
    public class Trial {
        public StreetSimCarManager.CarManagerStatus status;
        public int trialNum;
        [HideInInspector] public Collider colTrigger;
        [HideInInspector] public int startFrame;
        [HideInInspector] public float startTimestamp;
        [HideInInspector] public long startUnix;
        [HideInInspector] public int endFrame;
        [HideInInspector] public float endTimestamp;
        [HideInInspector] public long endUnix;
    }

    public static TrialController current;

    [Header("=== References ===")]
    public Transform xrCamera;
    public Collider xrCollider;
    public StreetSimCarManager carManager;
    public TrafficSignalController trafficController;
    public Collider northCollider, southCollider;

    [Header("=== Settings ===")]
    public Trial initialTrial;
    public List<Trial> trialOrder;
    [Space]
    public bool useInitialTrial = true;
    public bool randomizeOrder = true;

    [Header("=== Outputs ===")]
    public bool trialsActive = false;
    public Trial currentTrial;
    public Queue<Trial> trialQueue;
    public CSVWriter trialWriter;
    public CSVWriter positionWriter;

    private int trialCount = 0;

    void Awake() {
        current = this;
    }

    void Start() {
        // Ensure that a vehicle car manager ref is set
        if (carManager == null && StreetSimCarManager.CM != null) carManager = StreetSimCarManager.CM;
        // Same for traffic controller
        if (trafficController == null && TrafficSignalController.current != null) trafficController = TrafficSignalController.current;
        // Randomize order if shuffled
        if (randomizeOrder) trialOrder.ShuffleList();
        // if we want to use the initial trial, pre-pend the initial trial to the list of trials
        if (useInitialTrial) {
            trialOrder.Insert(0, initialTrial);
            trialCount = -1;
        }

        // Initialize the writers for trials and positions
        trialWriter.Initialize();
        positionWriter.Initialize();

        // Form queue from trials
        trialQueue = new Queue<Trial>(trialOrder);
        // Move down the list
        NextTrial();
    }

    public void NextTrial() {
        // If we have a current trial...
        if (trialsActive && currentTrial != null) {
            // Record its ending
            currentTrial.endUnix = CSVWriter.GetUnixTime();
            currentTrial.endTimestamp = Time.time;
            currentTrial.endFrame = Time.frameCount;
            // Write it in our writer
            trialWriter.AddPayload(currentTrial.trialNum);
            trialWriter.AddPayload(currentTrial.status.ToString());
            trialWriter.AddPayload($"{currentTrial.startUnix}");
            trialWriter.AddPayload(currentTrial.startTimestamp);
            trialWriter.AddPayload(currentTrial.startFrame);
            trialWriter.AddPayload($"{currentTrial.endUnix}");
            trialWriter.AddPayload(currentTrial.endTimestamp);
            trialWriter.AddPayload(currentTrial.endFrame);
            trialWriter.WriteLine(false);
        }

        // If there are no more trials, end all writers
        if (trialQueue.Count == 0) {
            trialWriter.Disable();
            positionWriter.Disable();
            trialsActive = false;
            return;
        }

        // Get the next trial in the queue
        trialsActive = true;
        trialCount += 1;
        currentTrial = trialQueue.Dequeue();
        currentTrial.trialNum = trialCount;
        currentTrial.startUnix = CSVWriter.GetUnixTime();
        currentTrial.startTimestamp = Time.time;
        currentTrial.startFrame = Time.frameCount;
        // Set the end collider based on the z-pos of the user
        if (xrCamera.position.z > 5f) {
            // Player is in the north. Set the end trigger to south
            currentTrial.colTrigger = southCollider;
        } else {
            currentTrial.colTrigger = northCollider;
        }

        // Set the car manager to the trial's car status
        carManager.SetCongestionStatus(currentTrial.status);
        // Set the traffic controller to reset
        trafficController.StartAtSessionIndex(0);
    }

    public void TrialCollision(Collider trialCollider, Collider otherCol) {
        if (trialCollider == currentTrial.colTrigger && otherCol == xrCollider) {
            // Transition to next trial ONLY if the trial collider is valid and the other collider is the player collider
            NextTrial();
        }
    }

    public void UpdatePosition(float t, int frame, string _name, int _guid, Vector3 p, Vector3 f) {
        if (positionWriter.is_active) {
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

public static class TrialExtensions {
    public static void ShuffleList<T>(this IList<T> ts) {
		var count = ts.Count;
		var last = count - 1;
		for (var i = 0; i < last; ++i) {
			var r = UnityEngine.Random.Range(i, count);
			var tmp = ts[i];
			ts[i] = ts[r];
			ts[r] = tmp;
		}
	}
}
