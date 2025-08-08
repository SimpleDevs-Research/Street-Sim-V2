using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityEditor;

//[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(ThirdPersonCharacter))]
public class StreetSimAgent : MonoBehaviour
{
    public enum AgentType {
        NPC,
        Model,
        Follower
    }

    [SerializeField] private ExperimentID id;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private ThirdPersonCharacter character;
    [SerializeField] private SkinnedMeshRenderer renderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider collider;
    [SerializeField] private Rigidbody rigidbody;
    private AgentHeadTurn headTurn;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepAudio;
    [SerializeField] private Vector3[] targetPositions; // note that the 1st position is the starting position
    private int currentTargetIndex = -1;
    [SerializeField] private bool shouldLoop, shouldWarpOnLoop;
    [SerializeField] private GameObject m_meshCopy = null;
    [SerializeField] private Component[] m_meshFollowers;

    private bool startingOnSouth = false;
    [SerializeField] private float m_originalSpeed = 0.4f;
    [SerializeField] private float m_crossDelayTime = 5f;
    [SerializeField] private float m_canCrossDelayTime = 0f;
    private bool m_canCrossDelayInitialized = false, m_canCrossDelayDone = false;
    [SerializeField] private bool m_canCross = false;

    [SerializeField] private StreetSimTrial.ModelBehavior behavior;
    [SerializeField] private StreetSimTrial.TrialDirection direction;
    [SerializeField] private StreetSimTrial.ModelConfidence confidence;

    private IEnumerator beCautiousCoroutine = null;
    private IEnumerator checkCarsCoroutine = null;
    public LayerMask carMask;

    private bool m_riskyButCrossing = false;
    public bool riskyButCrossing {
        get => m_riskyButCrossing;
        set {}
    }
    private AgentType m_agentType = AgentType.NPC;
    public AgentType agentType {
        get => m_agentType;
        set {}
    }

    
    public void GetAllChildren() {}

    
    public void GenerateMesh() {
        List<string> meshList = new List<string>() {
            "Root",
            "Spine1",
            "Spine2",
            "Chest",
            "Clavicle.L","Shoulder.L","Forearm.L","Hand.L",
            "Clavicle.R","Shoulder.R","Forearm.R","Hand.R",
            "Neck","Head",
            "Thigh.L","Shin.L","Foot.L","Toe.L",
            "Thigh.R","Shin.R","Foot.R","Toe.R"
        };
        GameObject newMeshObject = Instantiate(this.gameObject,Vector3.zero,Quaternion.identity, transform.parent);
        //PrefabUtility.UnpackPrefabInstance(newMeshObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
        DestroyImmediate(newMeshObject.GetComponent<ThirdPersonCharacter>());
        DestroyImmediate(newMeshObject.GetComponent<Rigidbody>());
        DestroyImmediate(newMeshObject.GetComponent<CapsuleCollider>());
        DestroyImmediate(newMeshObject.GetComponent<NavMeshAgent>());
        DestroyImmediate(newMeshObject.GetComponent<StreetSimAgent>());
        DestroyImmediate(newMeshObject.GetComponent<Animator>());
        FollowPosition follower = newMeshObject.AddComponent<FollowPosition>();
        follower.toFollow = this.transform;
        follower.offset = Vector3.up * -20f;
        MeshCollider col = newMeshObject.AddComponent<MeshCollider>();
        SkinnedMeshRendererHelper helper = newMeshObject.AddComponent<SkinnedMeshRendererHelper>();
        helper.meshRenderer = renderer;
        helper.collider= col;
        helper.updateDelay = 0.05f;
    }


    private void Awake() {
        //if (id == null) id = GetComponent<ExperimentID>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (character == null) character = GetComponent<ThirdPersonCharacter>();
        if (animator == null) animator = GetComponent<Animator>();
        if (rigidbody == null) rigidbody = GetComponent<Rigidbody>();
        headTurn = GetComponent<AgentHeadTurn>();

        targetPositions = new Vector3[2];
        Vector3 st = new Vector3(transform.position.x, 0f,transform.position.z);
        Vector3 en = new Vector3(transform.position.x, 0f, transform.position.z);
        if (transform.position.x >= 0f) {
            // We're on the east side, we need to move to the west side
            st.x = -70f;
            en.x = 70f;
        } else {
            st.x = 70f;
            en.x = -70f;
        }
        targetPositions[0] = st;
        targetPositions[1] = en;
        ManualInitialize();
    }

    private void Start() {
        agent.isStopped = false;
    }

    private void OnDrawGizmosSelected() {
        if (targetPositions.Length == 0) return;
        Gizmos.color = Color.blue;
        for(int i = 1; i < targetPositions.Length; i++) {
            Gizmos.DrawLine(targetPositions[i-1], targetPositions[i]);
        }
    }

    public void Initialize(
        Vector3[] targets, 
        StreetSimTrial.ModelBehavior behavior, 
        StreetSimTrial.ModelConfidence confidence,
        float speed,
        float canCrossDelay,
        bool shouldLoop, 
        bool shouldWarpOnLoop, 
        StreetSimTrial.TrialDirection direction,
        AgentType s_agentType
    ) {
        //targetPositions = targets;
        this.shouldLoop = shouldLoop;
        this.shouldWarpOnLoop = shouldWarpOnLoop;
        this.behavior = behavior;
        this.confidence = confidence;
        this.direction = direction;
        this.m_agentType = s_agentType;
        collider.enabled = true;
        rigidbody.isKinematic = false;
        agent.enabled = true;
        agent.speed = speed;
        m_originalSpeed = speed;
        character.enabled = true;
        animator.enabled = true;
        agent.isStopped = false;
        currentTargetIndex = -1;
        m_riskyButCrossing = false;
        StartCoroutine(WalkAnimationStepAudio());
        SetNextTarget();
    }

    public void ManualInitialize() {
        collider.enabled = true;
        rigidbody.isKinematic = false;
        agent.enabled = true;
        agent.speed = m_originalSpeed;
        character.enabled = true;
        animator.enabled = true;
        agent.isStopped = false;
        currentTargetIndex = 0;
        StartCoroutine(WalkAnimationStepAudio());
        SetNextTarget();
    }

    private IEnumerator WalkAnimationStepAudio() {
        animator.SetBool("Crouch",false);
        float prevFoot = Mathf.Sign(animator.GetFloat("JumpLeg"));
        float curFoot = 0;
        AudioClip footstep;
        while(agent.enabled) {
            curFoot = Mathf.Sign(animator.GetFloat("JumpLeg"));
            if (curFoot != prevFoot) {
                footstep = GetRandomFootstep();
                audioSource.PlayOneShot(footstep,1f);
            }
            prevFoot = curFoot;
            yield return null;
        }
        yield return null;
    }

    public AudioClip GetRandomFootstep() {
        int index = UnityEngine.Random.Range(0,footstepAudio.Length);
        return footstepAudio[index];
    }

    private void Update() {
        float dist = 0f, angleDiff;
        bool safe;
        if (targetPositions != null && targetPositions.Length != 0) {
            // Check distance betweenn current target and our position
            if (CheckDistanceToCurrentTarget(out dist)) {
                // We've reached our destination; setting new target
                SetNextTarget();
            }
        }
        character.Move(agent.desiredVelocity,false,false);
    }

    private void SetNextTarget() {
        if (currentTargetIndex == targetPositions.Length - 1) {
            // Reached the end, warp back to beginning and loop
            if (shouldLoop) {
                if (shouldWarpOnLoop) agent.Warp(targetPositions[0]);
                currentTargetIndex = 0;
                agent.SetDestination(targetPositions[currentTargetIndex]);
            } else {
                // Tell StreetSim to destroy this agent
                character.Move(Vector3.zero,false,false);
                DeactiveAgentManually();
                StreetSimAgentManager.AM.DestroyAgent(this);
            }
        } else {
            currentTargetIndex += 1;
            agent.SetDestination(targetPositions[currentTargetIndex]);
        }
    }

    private bool CheckDistanceToCurrentTarget(out float distance) {
        distance = Vector3.Distance(transform.position,targetPositions[currentTargetIndex]);
        return distance <= agent.stoppingDistance;
    }

    public SkinnedMeshRenderer GetRenderer() {
        return renderer;
    }

    
    public void DeactiveAgentManually() {
        //targetPositions = new Vector3[0];
        agent.enabled = false;
        character.enabled = false;
        animator.enabled = false;
        collider.enabled = false;
        rigidbody.isKinematic = true;
        headTurn.currentTargetTransform = null;
    }

    
    public ExperimentID GetID() {
        return id;
    }
    
}
