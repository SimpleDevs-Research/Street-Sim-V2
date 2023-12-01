using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Helpers;
using Random = UnityEngine.Random;

public class StreetSimAgentManager : MonoBehaviour
{

    public enum AgentManagerStatus { Off=0, NoCongestion=2, MinimalCongestion=4, SomeCongestion=6, Congestion=8 }

    [Header("Status")]
    public static StreetSimAgentManager AM;
    public enum CrossingCongestionStatus { Off=0, NoCongestion=2, MinimalCongestion=4, SomeCongestion=6, Congestion=8 }
    public enum BackgroundCongestionStatus { Off=0, NoCongestion=5, MinimalCongestion=10, SomeCongestion=20, Congestion=40 }
    public CrossingCongestionStatus crossingSetting = CrossingCongestionStatus.Off;
    public BackgroundCongestionStatus backgroundSetting = BackgroundCongestionStatus.Off;

    [Header("References")]
    [SerializeField] private Transform agentParent;
    public Transform WestLookAtTarget, EastLookAtTarget;
    [SerializeField] private AudioClip[] footstepAudio;

    [Header("Agents")]
    [SerializeField] private List<StreetSimAgent> agentPrefabs = new List<StreetSimAgent>();
    [SerializeField] private List<StreetSimAgent> crossingAgents = new List<StreetSimAgent>();
    [SerializeField] private List<StreetSimAgent> backgroundAgents = new List<StreetSimAgent>();
    private Queue<StreetSimAgent> crossingAgentsQueue = new Queue<StreetSimAgent>();
    private Queue<StreetSimAgent> backgroundAgentsQueue = new Queue<StreetSimAgent>();

    [Header("Colliders")]
    [SerializeField] private List<RemoteCollider> southColliders, northColliders;

    /*
    [SerializeField] private Transform idleTargetRef;

    [SerializeField] private List<StreetSimAgent> m_agents = new List<StreetSimAgent>();
    [SerializeField] private Queue<StreetSimAgent> m_inactiveAgents = new Queue<StreetSimAgent>();
    [SerializeField] private Queue<StreetSimAgent> m_waitingAgents = new Queue<StreetSimAgent>();
    [SerializeField] private List<StreetSimAgent> m_activeAgents = new List<StreetSimAgent>();

    [SerializeField] private List<StreetSimAgent> agentPrefabs = new List<StreetSimAgent>();
    [SerializeField] private List<StreetSimAgent> activeAgents = new List<StreetSimAgent>();
    [SerializeField] private List<NPCPath> nonModelPaths = new List<NPCPath>();
    [SerializeField] private List<NPCPath> modelPaths_NorthToSouth = new List<NPCPath>();
    [SerializeField] private List<NPCPath> modelPaths_SouthToNorth = new List<NPCPath>();
    private Dictionary<StreetSimTrial.TrialDirection, List<NPCPath>> modelPathsByDirection;
    [SerializeField] private List<NPCPath> modelPaths = new List<NPCPath>();

    private List<StreetSimAgent> m_currentModels = new List<StreetSimAgent>();
    public List<StreetSimAgent> currentModels { get=>m_currentModels; set{} }

    public Transform WestLookAtTarget, EastLookAtTarget;

    [SerializeField] private AudioClip[] footstepAudio;
    public LayerMask carDetectionLayerMask;

    [SerializeField] private List<Collider> southStartColliders = new List<Collider>();
    [SerializeField] private List<Collider> northStartColliders = new List<Collider>();

    [SerializeField] private List<Collider> southEndColliders = new List<Collider>();
    [SerializeField] private List<Collider> northEndColliders = new List<Collider>();
    */

    private void Awake() {
        AM = this;
        if (agentParent == null) agentParent = this.transform;

        /*
        m_inactiveAgents = new Queue<StreetSimAgent>(m_agents.Shuffle());
        modelPathsByDirection = new Dictionary<StreetSimTrial.TrialDirection, List<NPCPath>>(){
            { StreetSimTrial.TrialDirection.NorthToSouth, modelPaths_NorthToSouth },
            { StreetSimTrial.TrialDirection.SouthToNorth, modelPaths_SouthToNorth }
        };
        StartCoroutine(PrintAgents());
        */
    }

    public void SpawnBackgroundAgent(int side = 0) {
        // Stopgap based on our background setting
        if (backgroundAgents.Count + backgroundAgentsQueue.Count >= (int)backgroundSetting) return;
        // side == 0: random, -1 = south, 1 = north
        int sideclamp = Mathf.Clamp(side, -1, 1);
        switch(sideclamp) {
            case -1:
                SpawnBackgroundSouth();
                return;
            case 1:
                SpawnBackgroundNorth();
                return;
            default:
                float rand = Random.Range(0f, 1f);
                if (rand <= 0.5f) {
                    SpawnBackgroundSouth();
                } else {
                    SpawnBackgroundNorth();
                }
                return;            
        }
    }

    private void SpawnBackgroundSouth() {
        
    }
    private void SpawnBackgroundNorth() {

    }

    public void SpawnCrossingAgent() {

    }

    /*
    private void Update() {
        if (m_activeAgents.Count + m_waitingAgents.Count < numAgentValues[status]) QueueNextAgent();
    }
    public void QueueNextAgent() {
        if (m_inactiveAgents.Count == 0) return;
        StreetSimAgent nextAgent = m_inactiveAgents.Dequeue();
        m_waitingAgents.Enqueue(nextAgent);
    }

    private IEnumerator PrintAgents() {
        while(true) {
            // Return early if 1) we're off, 2) there are more than enough agents already active, or 3) there aren't any waiting agents anymore
            if (status == AgentManagerStatus.Off) {
                yield return null;
                continue;
            }
            if (m_activeAgents.Count >= numAgentValues[status]) {
                yield return null;
                continue;
            }
            if (m_waitingAgents.Count == 0) {
                yield return null;
                continue;
            }

            // Get the agent that we want from our queue
            StreetSimAgent agent = m_waitingAgents.Dequeue();
            //int newAgentIndex = (int)(Random.value * agentPrefabs.Count-1);

            // Randomly assign a path to this particular agent
            int newPathIndex = (int)(Random.value * nonModelPaths.Count-1);
            NPCPath newPath = nonModelPaths[newPathIndex];

            // Should the model move in the prescribed direction, or the oppoosite path? We randomly decide.
            Transform[] newPathTargets;
            if (Random.value < 0.5f) {
                newPathTargets = new Transform[newPath.points.Length];
                for(int i = newPath.points.Length-1; i >= 0; i--) {
                    newPathTargets[(newPath.points.Length-1)-i] = newPath.points[i];
                }
            } else {
                newPathTargets = newPath.points;
            }

            // We initialize the agent
            InitializeAgent(agent, newPathTargets);

            yield return new WaitForSeconds(1f);
        }
    }

    private void InitializeAgent(
        StreetSimAgent agent,
        Transform[] path,
        StreetSimTrial.ModelBehavior behavior = StreetSimTrial.ModelBehavior.Safe,
        StreetSimTrial.ModelConfidence confidence = StreetSimTrial.ModelConfidence.NotConfident,
        float speed = 0.4f,
        float canCrossDelay = 0f,
        bool shouldLoop = false,
        bool shouldWarpOnLoop = false,
        bool shouldAddToActive = true,
        StreetSimTrial.TrialDirection direction = StreetSimTrial.TrialDirection.NorthToSouth,
        StreetSimAgent.AgentType agentType = StreetSimAgent.AgentType.NPC
    ) {
        int upperBound = 3;
        int startIndex = 0;
        int endIndex = 0;
        Vector3 startPosition = path[0].position;
        Vector3 endPosition = path[path.Length-1].position;
        switch(direction) {
            case StreetSimTrial.TrialDirection.SouthToNorth:
                startIndex = Random.Range(0,southStartColliders.Count);
                startPosition = RandomPointInBounds(southStartColliders[startIndex].bounds);
                startPosition = new Vector3(startPosition.x, 0f, startPosition.z);
                endIndex = Random.Range(0,northEndColliders.Count);
                endPosition = RandomPointInBounds(northEndColliders[endIndex].bounds);
                break;
            default:
                startIndex = Random.Range(0,northStartColliders.Count);
                startPosition = RandomPointInBounds(northStartColliders[startIndex].bounds);
                endIndex = Random.Range(0,southEndColliders.Count);
                endPosition = RandomPointInBounds(southEndColliders[endIndex].bounds);
                break;
        }
        startPosition = new Vector3(startPosition.x, 0f, startPosition.z);
        endPosition = new Vector3(endPosition.x, 0f, endPosition.z);
        NavMeshPath tempPath = new NavMeshPath();
        NavMesh.CalculatePath(startPosition, endPosition, NavMesh.AllAreas, tempPath);
        Vector3[] newPath = new Vector3[tempPath.corners.Length];
        newPath[0] = startPosition;
        newPath[tempPath.corners.Length-1] = endPosition;
        for(int i = 1; i < tempPath.corners.Length-1; i++) {
            // randomize point
            Vector3 randomPoint;
            bool newPointFound = false;
            NavMeshHit hit;
            Vector3 dir = (tempPath.corners[i]-tempPath.corners[i-1]).normalized;
            do {
                randomPoint = tempPath.corners[i] + Random.insideUnitSphere * 1f;
                if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) {
                    // test direction. Must be in same or similar direction to original dir
                    newPointFound = Vector3.Dot(dir, (hit.position-tempPath.corners[i]).normalized) > 0;
                }
            } while(!newPointFound);
            newPath[i] = new Vector3(hit.position.x, 0f, hit.position.z);
        }
        agent.transform.position = newPath[0];
        Quaternion rotation = Quaternion.LookRotation((newPath[1]-newPath[0]).normalized, Vector3.up);
        agent.transform.rotation = rotation;
        agent.Initialize(newPath, behavior, confidence, speed, canCrossDelay, shouldLoop, shouldWarpOnLoop, direction, agentType);
        if (shouldAddToActive) m_activeAgents.Add(agent);
    }
    public void DestroyAgent(StreetSimAgent agent) {
        if (m_activeAgents.Contains(agent)) {
            m_activeAgents.Remove(agent);
            m_inactiveAgents.Enqueue(agent);
        }
        agent.transform.position = idleTargetRef.position;
    }

    public Vector3 RandomPointInBounds(Bounds bounds) {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    public void AddAgentManually(StreetSimAgent agent, int pathIndex, StreetSimTrial.ModelBehavior behavior = StreetSimTrial.ModelBehavior.Safe, float agentSpeed = 0.4f, bool isModel = false, StreetSimTrial.TrialDirection direction = StreetSimTrial.TrialDirection.NorthToSouth, StreetSimTrial.ModelConfidence confidence = StreetSimTrial.ModelConfidence.NotConfident) {
        //StreetSimAgent newAgent = default(StreetSimAgent);
        if (isModel) {
            List<NPCPath> availablePaths = modelPathsByDirection[direction];
            if (pathIndex < 0 && pathIndex > availablePaths.Count-1) {
                Debug.Log("[AGENT MANAGER] ERROR: path index does not exist among model paths");
                // return newAgent;
                return;
            }
            //DestroyModel();
            if (m_currentModels.Contains(agent)) DestroyAgent(agent);
            // PrintAgent(agent,modelPaths[pathIndex].points, out newAgent, behavior, false, false, false);
            InitializeAgent(agent, availablePaths[pathIndex].points, behavior, confidence, agentSpeed, 0.5f*m_currentModels.Count, false, false, false, direction, StreetSimAgent.AgentType.Model);
            //m_currentModel = newAgent;
            m_currentModels.Add(agent);
            // StreetSimModelMapper.M.MapMeshToModel(m_currentModel);
            //return newAgent;
            return;
        } else {
            if (pathIndex < 0 || pathIndex > nonModelPaths.Count-1) {
                Debug.Log("[AGENT MANAGER] ERROR: path index does not exist among non-model paths");
                return;
            }
            InitializeAgent(agent,nonModelPaths[pathIndex].points);
            // return newAgent;
            return;
        }
    }

    public void SetCongestionStatus(AgentManagerStatus newStatus, bool shouldReset = false) {
        status = newStatus;
        if (shouldReset && activeAgents.Count > 0) {
            Queue<StreetSimAgent> deleteQueue = new Queue<StreetSimAgent>(activeAgents);
            while(deleteQueue.Count > 0) {
                DestroyAgent(deleteQueue.Dequeue());
            }
        }
    }

    public void DestroyModels() {
        if (m_currentModels.Count == 0) return;
        foreach(StreetSimAgent model in m_currentModels)  {
            model.DeactiveAgentManually();
            DestroyAgent(model);
        }
        m_currentModels = new List<StreetSimAgent>();
    }


    */

    public void SetCongestionStatus(AgentManagerStatus newStatus, bool shouldReset = false) {}

    public void AddAgentManually(StreetSimAgent agent, int pathIndex, StreetSimTrial.ModelBehavior behavior = StreetSimTrial.ModelBehavior.Safe, float agentSpeed = 0.4f, bool isModel = false, StreetSimTrial.TrialDirection direction = StreetSimTrial.TrialDirection.NorthToSouth, StreetSimTrial.ModelConfidence confidence = StreetSimTrial.ModelConfidence.NotConfident) {}


    public List<StreetSimAgent> GetActiveAgents() {
        return new List<StreetSimAgent>();
    }

    public AudioClip GetRandomFootstep() {
        int index = UnityEngine.Random.Range(0,footstepAudio.Length);
        return footstepAudio[index];
    }

    public void DestroyModels() {}

    public void DestroyAgent(StreetSimAgent agent) {}
}
