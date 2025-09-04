using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;

public class RVORobot : MonoBehaviour
{

    [Header("=== REFERENCES ===")]
    public RVOSceneManager manager = null;
    public MeshRenderer meshRenderer;
    public Transform destinationRef = null;
    
    [Header("=== CONFIGS ===")]
    public RVO.RVOProfile profile;
    public float maxSpeed;
    public float radius;
    public Vector2 radiusRange;
    public float aggressiveness;
    public Color color;

    [Header("=== MOVEMENT DATA ===")]
    [ReadOnlyInsp] public int index;
    [ReadOnlyInsp] public int gridCellIndex = -1;
    [ReadOnlyInsp] public Vector2 position;
    public Vector3 position3D => this.position.ToVector3();
    [ReadOnlyInsp] public Vector2 velocity;
    public Vector3 velocity3D => this.velocity.ToVector3();

    [Header("=== RUNTIME METRICS ===")]
    [SerializeField, ReadOnlyInsp] private bool instantiated = false;
    [SerializeField, ReadOnlyInsp] private int numNeighbors;
    [SerializeField, ReadOnlyInsp] private Vector2 destination;
    public Vector3 destination3D => this.destination.ToVector3();
    [SerializeField, ReadOnlyInsp] private Vector2 desiredVelocity;
    public Vector3 desiredVelocity3D => this.desiredVelocity.ToVector3();
    [SerializeField, ReadOnlyInsp] private Vector2 optimalVelocity;
    public Vector3 optimalVelocity3D => this.optimalVelocity.ToVector3();
    private int numIterations; 
    private List<RVO_BA> RVO_BA_ALL; 
    private List<Vector2> dirs;
    
    [Header("=== DEBUG ===")]
    public bool gizmos = true;

    void OnDrawGizmos() {
        if (!Application.isPlaying) {
            // Draw a sphere, square, and line connecting the two
            Gizmos.color = this.profile.color;
            Gizmos.DrawSphere(transform.position, this.profile.radius*2f);
            if (this.destinationRef != null) {
                Gizmos.DrawCube(this.destinationRef.position, new Vector3(0.16f, 0.1f, 0.16f));
                Gizmos.DrawLine(transform.position, this.destinationRef.position);
            }
            return;
        }
    }

    public void SetProfile(RVO.RVOProfile p) {  this.profile = p;           }
    public void SetColor(Color c) {         this.profile.color = c;         }
    public void SetRadius(float r) {        this.profile.radius = r;        }
    public void SetRadiusRange(Vector2 r) { this.profile.radiusRange = r;   }
    public void SetMaxSpeed(float ms) {     this.profile.maxSpeed = ms;     }

    public void Initialize(int index, float maxSpeed, float radius, Vector2 radiusRange, float aggressiveness) {
        this.index = index;
        this.position = this.transform.position.ToVector2();
        this.velocity = Vector2.zero;
        
        this.maxSpeed = maxSpeed;
        this.radius = radius;
        this.radiusRange = radiusRange;
        this.aggressiveness = aggressiveness;

        this.RVO_BA_ALL = new List<RVO_BA>();
        this.numIterations = 0;
        
        this.instantiated = true;
    }

    public void SetDestination(Vector2 target) {
        this.destination = target;
    }
    public void SetDestination(Vector3 target) {
        this.destination = target.ToVector2();
    }
    
    public void SetDestinationRef(Transform d) {
        this.destinationRef = d;
        this.SetDestination(d.position);
    }

    public void ChangeColor() {
        if (this.meshRenderer == null) this.meshRenderer = GetComponent<MeshRenderer>();
        this.meshRenderer.materials[0].SetColor("_Color", this.color);
        if (this.destinationRef != null) {
            MeshRenderer r = this.destinationRef.gameObject.GetComponent<MeshRenderer>();
            if (r != null) r.materials[0].SetColor("_Color", this.color);
        }
    }

    public void GetDesiredVelocity() {
        // If the destinationRef is not null, we have to update the destination
        if (this.destinationRef != null) this.SetDestination(this.destinationRef.position);
        // The desired velocity is essentially a direct vector towards our current target
        Vector2 diff = this.destination - this.position;
        // Depending on how far away we are from the target, we either set to the desired direction capped to maxSpeed
        if (Vector2.Distance(this.position, this.destination) > 0.1f) {
            this.desiredVelocity = diff.normalized * Mathf.Clamp(diff.magnitude, 0f, this.maxSpeed);
            return;
        }
        // ... or we set to 0 because we don't need to move anymore
        this.desiredVelocity = Vector2.zero;
    }

    public void UpdateRobot(RVORobot[] neighbors, Vector2[] directions, float deltaTime, bool locomote = true) {
        // Skip the update if the robot hasn't been updated
        if (!this.instantiated) return;

        // Get and store the position of the transfom
        this.position = this.transform.position.ToVector2();

        // Set the # of recorded neighbors    
        this.numNeighbors = neighbors.Length;
        
        // Only update if enough iterations have passed
        if (this.numIterations % 3 == 0) {
            // Get the desired velocity (capped to maxSpeed)
            this.GetDesiredVelocity();
            // We gotta do the RVO
            this.optimalVelocity = this.GetRVO(
                this.index, this.position, this.velocity, this.radius, this.desiredVelocity, this.maxSpeed, this.aggressiveness,
                neighbors, directions, deltaTime, out this.RVO_BA_ALL, out this.dirs);
        }
        
        // If we gotta locomote, locomote
        if (locomote) MoveRobot(deltaTime);
        
        // Update numiterations
        this.numIterations += 1;
        // Adjust our radius based on a min-max radius field and current velocity
        float radiusRatio = this.velocity.magnitude / this.maxSpeed;
        this.radius = (1f - radiusRatio) * this.radiusRange.x + radiusRatio * this.radiusRange.y;
        // Update our gameobject position
        this.transform.position = this.position3D;
    }

    private Vector2 GetRVO(
        int index, Vector2 pA, Vector2 vA, float radius, Vector2 vDesired, float maxSpeed, float aggressiveness,
        RVORobot[] neighbors, Vector2[] directions, float deltaTime, out List<RVO_BA> RVO_BA_ALL, out List<Vector2> dirs
    ) {
        // Calculate the Minkowski sum beforehand
        float MINKOWSKI_RAD = 2f * radius;
        
        // Generate list of all RVO regions
        RVO_BA_ALL = new List<RVO_BA>();
        
        // Right now, the optimal velocity is the desired velocity. This is so that if there are no other robots, we simply doot-doot to our desired velocity
        Vector2 optimalVelocity = vDesired;
        
        // Copy the list of directions
        List<Vector2> suitableDirections = new List<Vector2>(directions);
        
        // Generate list of penalties
        float[] timeCost = new float[neighbors.Length];
        int[] directionRobotMap = new int[directions.Length];
        
        // Sort this list in order of closest to our desired velocity
        suitableDirections.Sort((v1,v2)=>(Vector2.Dot(vDesired,v2)).CompareTo(Vector2.Dot(vDesired,v1)));
        
        // With that out of the way, we can begin to compare between this robot and all other robots
        // Loop through all robots
        Vector2 pB, vB, diff_BA, diff_BA_norm;
        float dist_BA;
        bool desiredIsSuitable = true;
        
        for(int i = 0; i < neighbors.Length; i++) {
            // skip if we're meeting ourselves
            if (neighbors[i].index == index) continue;

            // Copy suitableDirections into a temp one
            List<Vector2> tempSuitable = new List<Vector2>(suitableDirections);

            pB = neighbors[i].position;        // position of the other robot
            vB = neighbors[i].velocity;        // velocity of the other robot
                
            // Calculate the translation from pA to the new "triangle space"
            Vector2 translate_pA = pA + vB;                // VO
            //Vector2 translate_pA = this.position + 0.5f*(vB + this.velocity);      // RVO
                
            // Calculate the left and right bounds of the Minkowski Sum
            // Step 1: Get the distance between the two positions and the angle between the two of them, relative to ----> axis
            diff_BA = pB - pA;
            dist_BA = diff_BA.magnitude;
            diff_BA_norm = diff_BA.normalized;
            float theta_BA = Mathf.Atan2(diff_BA.y, diff_BA.x);
            
            // Step 2: Restrict the lower bound of the distance between the two to that of 2x the robot radius
            if (MINKOWSKI_RAD > dist_BA) dist_BA = MINKOWSKI_RAD;
            
            // Step 3: Get the angle between the direct vector towards B and the outer left and right vectors that are tangential
            float theta_BAort = Mathf.Asin(MINKOWSKI_RAD / dist_BA);
            
            // Step 4: Get the left and right tangential vectors to B that represent the pyramid from A to B's sides
            float theta_ort_left = theta_BA + theta_BAort;
            Vector2 bound_left = new Vector2(
                Mathf.Cos(theta_ort_left), 
                Mathf.Sin(theta_ort_left)
            );
            float theta_ort_right = theta_BA - theta_BAort;
            Vector2 bound_right = new Vector2(
                Mathf.Cos(theta_ort_right), 
                Mathf.Sin(theta_ort_right)
            );

            // Calculate the time cost for this robot
            timeCost[i] = dist_BA / maxSpeed;

            // Right now, we have the left and right bounds, as well as the M.Sum relative to pB.
            // We need to check 2 things:
            // 1) Get the angles (relative to the -----> positive x_axis)
            // 2) if we look at our current desired velocity, will it be in RVO?
            // 3) For all vertices in suitableVelocities, will they be in RVO?

            // 1)
            float theta_right = Mathf.Atan2(bound_right.y, bound_right.x);
            float theta_left = Mathf.Atan2(bound_left.y, bound_left.x);

            // 2) Check if the desired velocity is in (R)VO
            //if (VelInVO(translate_pA, this.position, theta_left, theta_right, this.desiredVelocity)) {
            if (VelInVO(translate_pA, pA, theta_left, theta_right, 2f*vDesired-vA)) {
                desiredIsSuitable = false;
            }

            // 3) For all suitable vectors, check if they'll be in the (R)VO
            for(int j = 0; j < suitableDirections.Count; j++) {
                // Get the velocity
                Vector2 potential = suitableDirections[j];
                Vector2 potentialCapped = potential.normalized * Mathf.Clamp(potential.magnitude, 0f, maxSpeed);
                //if (VelInVO(translate_pA, this.position, theta_left, theta_right, potentialCapped)) {
                if (VelInVO(translate_pA, pA, theta_left, theta_right, 2f*potentialCapped-vA)) {
                    // In this case, this is not a suitable velocity. Kill it off while we can
                    int dirIndex = System.Array.IndexOf(directions, potential);
                    directionRobotMap[dirIndex] = i;
                    tempSuitable.Remove(potential);
                }
            }

            // Save the remaining suitable directions
            suitableDirections = tempSuitable;

            // Might as well add bound data to rvo_all
            // RVO
            //Vector2 transl_vB_vA = this.position + 0.5f * (vB + this.velocity);
            RVO_BA_ALL.Add(new RVO_BA(translate_pA, bound_left, bound_right, pB, vB, dist_BA, MINKOWSKI_RAD));
        }
            
        // Now with all other objects processed, we can determine an "optimal" velocity
        // We make sure to save the suitable directions
        dirs = suitableDirections;
        
        // If our desired is suitable, then just go with it
        if (desiredIsSuitable) return vDesired;
        
        // If not desirable, we get the closest in suitableDirections
        if (suitableDirections.Count > 0) return suitableDirections[0].normalized * Mathf.Clamp(suitableDirections[0].magnitude, 0f, maxSpeed);
        
        // If no other options, we are out of options. We must determine a new velocity that is penalized.
        Vector2 bestVelocityWithPenalty = directions[0].normalized * maxSpeed;
        float bestPenalty = GetPenalty(vDesired, bestVelocityWithPenalty, timeCost[directionRobotMap[0]], aggressiveness);
        for(int j = 1; j < directions.Length; j++) {
            // Get the capped vel
            Vector2 tempDirVel = directions[j].normalized * maxSpeed;
            float tempPenalty = GetPenalty(vDesired, tempDirVel, timeCost[directionRobotMap[j]], aggressiveness);
            if (tempPenalty < bestPenalty) {
                bestVelocityWithPenalty = tempDirVel;
                bestPenalty = tempPenalty;
            }
        }
        return bestVelocityWithPenalty;
    }

    private static bool VelInVO(Vector2 p0, Vector2 pA, float theta_left, float theta_right, Vector2 vQuery) {
        // Get a new velocity based on the translation
        Vector2 dif = vQuery + pA - p0;
        // Calculate the angles involved, all relative to the + x-axis
        float theta_dif = Mathf.Atan2(dif.y, dif.x);
        // Check if the angle of dif is between the angle of theta_right and theta_left
        return RVO.Utils.InBetween(theta_dif, theta_left, theta_right);
    }

    private static float GetPenalty(Vector2 vDesired, Vector2 vQuery, float timeCost, float aggressiveness = 1f) {
        return aggressiveness/timeCost + (vDesired - vQuery).magnitude;
    }
    
    private void MoveRobot(float t) {
        if (Vector2.Distance(this.position, this.destination) < 0.1f) {
            this.velocity = Vector2.zero;
            this.position = this.destination;
            return;
        }
        this.velocity = this.optimalVelocity;
        this.position += this.velocity * t;
    }
}
