using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExperimentController : MonoBehaviour
{
    [System.Serializable]
    public class ExperimentStep {
        public string stepName = "";
        public UnityEvent stepEvent;
        public string instruction;
        public List<GameObject> activateObjects = new List<GameObject>();
        public List<GameObject> deactivateObjects = new List<GameObject>();
    }

    [SerializeField]
    private bool initializeOnStart = false;
    private bool started = false;

    [SerializeField]
    private Vector4 dimensions = Vector4.zero;

    public delegate void ExperimentStepDelegate();
    public List<ExperimentStep> experimentSteps = new List<ExperimentStep>();
    private int currentStepIndex = -1;

    [SerializeField]
    private Transform centerEye = null;
    [SerializeField]
    private InstructionsUI instructionsTextbox = null;

    [SerializeField]
    private MoveWithJoystick heightReticle;
    [SerializeField]
    private GetHeightFromFloor heightFromFloor;

    // Start is called before the first frame update
    private void Start() {
        if (initializeOnStart) StartExperiment();
    }

    public void NextStep() {
        if (currentStepIndex < experimentSteps.Count-1 && CheckValidToContinue()) {
            currentStepIndex += 1;
            experimentSteps[currentStepIndex].stepEvent?.Invoke();
            if (instructionsTextbox != null) instructionsTextbox.SetText(experimentSteps[currentStepIndex].instruction);
            if (experimentSteps[currentStepIndex].activateObjects.Count > 0) {
                foreach(GameObject go in experimentSteps[currentStepIndex].activateObjects) {
                    go.SetActive(true);
                }
            }
            if (experimentSteps[currentStepIndex].deactivateObjects.Count > 0) {
                foreach(GameObject go in experimentSteps[currentStepIndex].deactivateObjects) {
                    go.SetActive(false);
                }
            }
        }
    }
    public void SetStep(int stepIndex) {
        if (CheckValidToContinue()) {
            currentStepIndex = stepIndex;
            experimentSteps[currentStepIndex].stepEvent?.Invoke();
            if (instructionsTextbox != null) instructionsTextbox.SetText(experimentSteps[currentStepIndex].instruction);
            if (experimentSteps[currentStepIndex].activateObjects.Count > 0) {
                foreach(GameObject go in experimentSteps[currentStepIndex].activateObjects) {
                    go.SetActive(true);
                }
            }
            if (experimentSteps[currentStepIndex].deactivateObjects.Count > 0) {
                foreach(GameObject go in experimentSteps[currentStepIndex].deactivateObjects) {
                    go.SetActive(false);
                }
            }
        }
    }

    private bool CheckValidToContinue() {
        if (centerEye == null) {
            Debug.LogError("ERROR: `centerEye` cannot be null.");
            return false;
        }
        return true;
    }

    public void StartExperiment() {
        if (started) return;
        else if (!CheckValidToContinue()) return;
        started = true;
        SetStep(0);
    }
    
    public void PositionReticle() {
        heightReticle.SetPosition(new Vector3(0f, heightFromFloor.height, 5f));
        Debug.Log("Positioning Reticle!");
    }

    public void ConfirmLeftLimit() {
        Debug.Log("Confirming Left Limit!");
    }
    public void ConfirmRightLimit() {
        Debug.Log("Confirming Right Limit!");
    }
    public void ConfirmTopLimit() {
        Debug.Log("Confirming Top Limit!");
    }
    public void ConfirmBottomLimit() {
        Debug.Log("Confirming Bottom Limit!");
    }
}
