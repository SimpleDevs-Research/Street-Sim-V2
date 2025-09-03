using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using RVO;

public class RVORobotGenerator : MonoBehaviour
{
    public enum GeneratorPrefabs { Circle }

    [Header("=== REFERENCES ===")]
    public RVOSceneManager manager = null;
    public GameObject robotPrefab = null;
    public GameObject targetPrefab = null;

    [Header("=== GENERATOR SETTINGS ===")]
    public int nRobots = 10;
    public GeneratorPrefabs format = GeneratorPrefabs.Circle;
    public bool generateTargets = false;
    public float distanceFromCenter = 1f;

    [Header("=== ROBOT DEFAULTS ===")]
    public RVO.RVOProfile defaultProfile;
    public bool randomizeColor = true;
    
    private GameObject robotParent = null, targetParent = null;
    private List<RVORobot> robots = new List<RVORobot>();

    public void GenerateRobots() {
        switch(format) {
            case GeneratorPrefabs.Circle:
                GenerateCircle();
                break;
        }
    }

    private void GenerateCircle() {
        Vector2[] positions = RVO.Utils.CreateDirections2D(nRobots, distanceFromCenter);
        InstantiateRobots(positions);
    }

    private void InstantiateRobots(Vector2[] positions) {
        // Delete existing robots, but not the parents
        DeleteRobots(false);
        if (robotParent == null) robotParent = new GameObject("RobotParent");
        if (generateTargets && targetParent == null) targetParent = new GameObject("TargetParent");
        
        // Prepare some variables
        GameObject newRobot, newTarget;
        RVORobot r;
        Color c;
        Vector3 targetPos;
        RVO.RVOProfile profile;

        // Generate robots at each position
        for(int i = 0; i < positions.Length; i++) {
            // Generate the RVORobot GameObject
            if (robotPrefab != null) {
                newRobot = Instantiate(robotPrefab, positions[i].ToVector3(), Quaternion.identity);
            } else {
                newRobot = new GameObject($"Robot{i+1}");
                newRobot.transform.position = positions[i].ToVector3();
                newRobot.AddComponent<RVORobot>();
            }

            // Set the parent, and get the reference to the RVORobot 
            newRobot.transform.parent = robotParent.transform;
            r = newRobot.GetComponent<RVORobot>();

            // If needed, generate a reciprocal target
            if (generateTargets) {
                // Depending on choice of format, the position of the target's position will differ
                switch(format) {
                    case GeneratorPrefabs.Circle:
                        targetPos = -positions[i].ToVector3();
                        break;
                    default:
                        targetPos = -positions[i].ToVector3();
                        break;
                }
                // If the prefab doesn't exist, create it. Otherwise, use the prefab
                if (targetPrefab != null) {
                    newTarget = Instantiate(targetPrefab, targetPos, Quaternion.identity);
                } else {
                    newTarget = new GameObject($"Target{i+1}");
                    newTarget.transform.position = targetPos;
                }
                // Set the target's parent, and set the newborn robot's destination reference to this target
                newTarget.transform.parent = targetParent.transform;
                r.SetDestinationRef(newTarget.transform);
            }

            // Need to set the profile
            profile = defaultProfile.clone();
            // Modify the color in the profile if called for
            if (randomizeColor) profile.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            // Add the profile to the robot
            r.SetProfile(profile);

            robots.Add(r);
        }

        if (manager != null) manager.SetRobots(robots);
    }

    public void DeleteRobots(bool deleteParent = true) {
         while(robots.Count > 0) {
            if (robots[0] != null) {
                RVORobot r = robots[0];
                if (r.destinationRef != null) DestroyImmediate(r.destinationRef.gameObject);
                DestroyImmediate(r.gameObject);
            }
            robots.RemoveAt(0);
        }
        robots = new List<RVORobot>();
        if (robotParent != null && deleteParent) {
            DestroyImmediate(robotParent);
            robotParent = null;
        }
        if (targetParent != null && deleteParent) {
            DestroyImmediate(targetParent);
            targetParent = null;
        }
    }
}
