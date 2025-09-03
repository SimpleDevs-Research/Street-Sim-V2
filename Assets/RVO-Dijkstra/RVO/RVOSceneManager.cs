using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;

public class RVOSceneManager : MonoBehaviour
{

    [Header("=== REFERENCES ===")]
    [SerializeField, ReadOnlyInsp] private List<RVORobot> _robots = new List<RVORobot>();

    [Header("=== SETTING UP THE GRID ===")]
    [SerializeField] private RVO.Grid.Grid2D<RVORobot> grid;
    public Transform lowerBoundRef = null, upperBoundRef = null;
    public void SetGridBounds() {
        if (lowerBoundRef == null || upperBoundRef == null) return;
        grid.SetBounds(lowerBoundRef.position, upperBoundRef.position);
    }
    public void InitializeGrid() {
        bool successful = grid.Initialize();
        if (!successful) {
            Debug.LogError("ERROR: Cannot initialize grid. Are you missing bounds?");
        }
    }


    /*
    [Header("=== REFERENCES ===")]
    public Grid2D grid;
    public Transform lowerTransform, upperTransform;
    public Transform sceneAgentParent;

    [Header("=== CONFIGS ===")]
    public float robotRadius = 0.25f;
    public float maxSpeed = 1f;
    public float deltaTime = 0.0165f;
    public Vector2 robotRadiusRange = new Vector2(0.125f,0.25f);
    public int num_pts = 100;
    public IsVirtual isVirtual = IsVirtual.VirtualAgents;
    public bool autoGenerateRobots = true;

    [Header("=== OUTPUTS ===")]
    [SerializeField] private Vector2[] directions;
    public RVO_Robot[] robots; 

    [Header("=== DEBUG ===")]
    public bool gizmo_directions = true;
    public bool gizmo_minkowski = true;
    public bool gizmo_bounds = true;
    public bool gizmo_suitable = true;
    public bool gizmo_rvo => gizmo_minkowski || gizmo_bounds || gizmo_suitable;

    void OnDrawGizmos() {
        if (lowerTransform == null || upperTransform == null) return;

        Vector3 center = (upperTransform.position + lowerTransform.position)/2f;
        Vector3 size = upperTransform.position - lowerTransform.position;
        
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(center, size);

        if (gizmo_directions && directions != null && directions.Length > 0) {
            Gizmos.color = Color.yellow;
            for(int i = 0; i < directions.Length; i++) {
                Vector3 direction3D = new Vector3(directions[i].x, 0f, directions[i].y);
                Gizmos.DrawSphere(center + direction3D, 0.1f);
            }
        }

        if (Application.isPlaying) {
            for(int i = 0; i < robots.Length; i++) {
                if (!robots[i].gizmos) continue;
                Gizmos.color = robots[i].color;
                Gizmos.DrawSphere(robots[i].position3D, robots[i].radius);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(robots[i].position3D, robots[i].velocity3D);

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(robots[i].destination3D, 0.1f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(robots[i].position3D, robots[i].destination3D);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(robots[i].position3D, robots[i].desiredVelocity3D);
                Gizmos.color = Color.black;
                Gizmos.DrawRay(robots[i].position3D, robots[i].optimalVelocity3D);

                // Render RVOs
                if (gizmo_rvo && robots[i].RVO_BA_ALL.Count > 0) {
                    if (gizmo_minkowski) {
                        Gizmos.color = robots[i].color;
                        Gizmos.DrawWireSphere(robots[i].position3D, 2f * (robots[i].radius + 0.1f));
                    }
                    if (gizmo_bounds) {
                        Gizmos.color = new Color(robots[i].color.r, robots[i].color.g, robots[i].color.b, 0.25f);
                        foreach(RVO_BA ba in robots[i].RVO_BA_ALL) {
                            Gizmos.DrawRay(robots[i].position3D, new Vector3(ba.bound_left.x, 0f, ba.bound_left.y) * ba.dist_BA);
                            Gizmos.DrawRay(robots[i].position3D, new Vector3(ba.bound_right.x, 0f, ba.bound_right.y) * ba.dist_BA);
                        }
                    }
                    if (gizmo_suitable) {
                        foreach(Vector2 dir in robots[i].dirs) {
                            Vector3 dir3D = new Vector3(dir.x, 0f, dir.y);
                            Gizmos.DrawRay(robots[i].position3D, dir3D);
                        }
                    }
                }
            }
        }
    }
    */
    
    /*
    private void Awake() {
        if (directions == null || directions.Length == 0) CreateDirections();       // Generate directions if we haven't already
        if (sceneAgentParent == null) sceneAgentParent = this.transform;            // set scene agent parent if not set
        if (grid != null) {
            grid.SetRefs(lowerTransform, upperTransform);
            grid.GenerateGrid();
        }
        // We only auto-generate robots if our robots list is empty
        InitializeRobots();
    }

    // Update is called once per frame
    private void Update() {
        
        // We manually update the grid
        if (grid != null) {
            grid.ClearGrid();
            for(int i = 0; i < robots.Length; i++) robots[i].gridCellIndex = grid.UpdateCell(robots[i]);
        }

        // We need to update each robot.
        // note that we'll be using neighbor-based robots, not looking at all robots. Much faster that way
        RVO_Robot[] neighborRobots;
        float dt = (deltaTime < 0f) ? Time.deltaTime : deltaTime;

        // Loop through robots
        for(int i = 0; i < robots.Length; i++) {
            // generate the list of neighbor robots
            neighborRobots = (grid != null) 
                ? grid.cells[robots[i].gridCellIndex].GetNeighborAgents().ToArray()
                : robots;
            // Update the robot
            robots[i].UpdateRobot(neighborRobots, directions, dt, true);
        }
    }

    // Function to generate the ring of directions that we will apply to each robot when they're looking for an opitmal velocity direction
    public void CreateDirections() {
        directions = RVO.Utils.CreateDirections2D(num_pts);
    }

    // This function generates robots. Can be called manually to pre-populate the robots array
    public void InitializeRobots() {
        
        // Prepare some values that will help with generating robots
        Vector3 size = upperTransform.position - lowerTransform.position;
        GameObject sceneRobot;
        Vector3 pos;
        Color color;
        float aggressiveness;
        
        if (autoGenerateRobots) {
            // if we DO have robots that are currently existing in the scene, then we have to delete them from the scene
            foreach(RVO_Robot robot in robots) {
                if (robot.transformRef != null) Destroy(robot.transformRef.gameObject);
            }
            // Initialize the robots array. They're currently empty though. We need to initialize them
            robots = new RVO_Robot[numRobots];
        }

        // Generate the robots using a `for` loop
        for(int i = 0; i < numRobots; i++) {
            // Because this is dealing with robot creation, we'll have to randomly generate a position and color
            pos = lowerTransform.position + new Vector3(Random.value * size.x, 0f, Random.value * size.z);
            color = new Color(Random.value, Random.value, Random.value, 1f);
            aggressiveness = Random.value + 1f;

            // Generate the robot. Velocity is set to a zero vector
            if (autoGenerateRobots) {
                robots[i] = new RVO_Robot(i, pos, Vector3.zero, maxSpeed, robotRadius, robotRadiusRange, aggressiveness, color);
            } else {
                robots[i].Initialize(i, pos, Vector3.zero, maxSpeed, robotRadius, robotRadiusRange, aggressiveness, color);
            }

            // If we're using scene agents, we need to create scene gameobjects for each robot
            if (autoGenerateRobots) {
                if (isVirtual == IsVirtual.SceneAgents) {
                    sceneRobot = new GameObject($"Robot_{i}");
                    sceneRobot.transform.parent = sceneAgentParent;
                    sceneRobot.transform.position = pos;
                    robots[i].transformRef = sceneRobot.transform;
                }
                // Because we're generating the robots, we also need to generate random target for each robot
                Vector3 targetPos = lowerTransform.position + new Vector3(Random.value * size.x, 0f, Random.value * size.z);
                robots[i].SetDestination(targetPos);
            }

            // Calculate desired velocity
            robots[i].GetDesiredVelocity();

            // If grid is not null, get the current projected index
            if (grid != null) robots[i].gridCellIndex = grid.UpdateCell(robots[i]);
        }
    }
    */

    public void SetRobots(List<RVORobot> r) {
        _robots = r;
    }
}
