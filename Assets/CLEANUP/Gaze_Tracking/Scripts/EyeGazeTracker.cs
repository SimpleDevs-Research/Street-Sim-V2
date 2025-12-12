using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EyeGazeTracker : MonoBehaviour
{
    [Header("=== Writing Settings ===")]
    public bool activateOnStart = true;
    public bool force90FPS = true;
    public CSVWriter writer;
    [Space]
    [SerializeField] private float startTime = 0f;

    // =======================
    [Header("=== References ===")]
    public Camera leftCamera;
    public Camera rightCamera;
    public Camera centerCamera;
    public Transform leftEyeGaze, rightEyeGaze;
    public Renderer leftEyeCursor, rightEyeCursor, centerEyeCursor;

    // =======================
    [Header("=== Gaze Settings ===")]
    public float gazeDistance = 100f;
    public float cursorRefScale;
    public LayerMask layersToInclude;
    [SerializeField] private bool isWriting = false;

    private void Awake() {
        if (force90FPS) Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(90f);
        if (activateOnStart) Activate();
    }

    private void Update() {
        if (!isWriting) return;
        RecordGaze();
    }

    void OnDestroy() {
        if (writer.is_active) Deactivate();
    }

    public void Activate() {
        if (writer.is_active) {
            Debug.LogError("Cannot re-activate Eye Gaze Tracker: writer is already active");
            return;
        }
        // Wait until writer is active
        if (writer.Initialize()) {
             // Initialize start time
            startTime = Time.time;
            // Add a single row to represent the start of the recording.
            RecordEvent("Activation");
            // Let update loop take over
            isWriting = true;
        }
    }

    public void Deactivate() {
        isWriting = false;
        // Add final line
        RecordEvent("Deactivate");
        // Disable writer
        writer.Disable();
    }

    // Event Logistics only
    public void RecordEvent(string event_description) { 
        RecordEvent(
            // Event Logistics
            event_description,
            // Gaze Data
            Vector3.zero, Vector3.zero, Vector3.zero, 0f, "",
            // Head Data
            Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, 0f, "",
            // Gaze vs Head
            Vector3.zero, 0f
        ); 
    }
    public void RecordEvent(
            // Event Logistics
            string event_description,
            // Gaze Data
            Vector3 gaze_target_world_pos,
            Vector3 gaze_target_screen_pos,
            Vector3 gaze_direction,
            float gaze_target_distance,
            string gaze_target_name,
            // Head Data
            Vector3 head_target_world_pos,
            Vector3 head_target_screen_pos,
            Vector3 head_direction,
            Vector3 head_position,
            float head_target_distance,
            string head_target_name,
            // Gaze vs Head
            Vector3 gaze_head_rel_direction,
            float gaze_head_angle_diff
    ) {
        // Event Logistics
        writer.AddPayload(GetCurrentTime());
        writer.AddPayload(Time.frameCount);
        writer.AddPayload(event_description);
        // Gaze Data
        writer.AddPayload(gaze_target_world_pos); 
        writer.AddPayload(gaze_target_screen_pos);
        writer.AddPayload(gaze_direction);
        writer.AddPayload(gaze_target_distance);
        writer.AddPayload(gaze_target_name);
        // Head Data
        writer.AddPayload(head_target_world_pos);
        writer.AddPayload(head_target_screen_pos);
        writer.AddPayload(head_direction);
        writer.AddPayload(head_position);
        writer.AddPayload(head_target_distance);
        writer.AddPayload(head_target_name);
        // Gaze vs Head
        writer.AddPayload(gaze_head_rel_direction);
        writer.AddPayload(gaze_head_angle_diff);
        // Write Line
        writer.WriteLine(true);
    }

    private void RecordGaze() {
        // ================= //
        // === Gaze Data === //
        // ================= //
        
        // 1. Initialize gaze data for recording
        string gaze_event = "";
        Vector3 gaze_direction = Vector3.Normalize((leftEyeCursor.transform.position + rightEyeCursor.transform.position)/2f - centerCamera.transform.position);
        Vector3 gaze_target_world_position = centerCamera.transform.position + (gaze_direction * gazeDistance);
        float gaze_target_distance = gazeDistance;
        string gaze_target_name = "";

        // 2. Use a raycast to detect the hit.
        RaycastHit gaze_hit;
        if (Physics.Raycast(centerCamera.transform.position, gaze_direction, out gaze_hit, gazeDistance, layersToInclude)) {
            gaze_event = "Eye Hit";
            gaze_target_world_position = gaze_hit.point;
            gaze_target_distance = gaze_hit.distance;
            gaze_target_name = gaze_hit.transform.gameObject.name;
        }

        // 3. Render the position of the center eye cursor
        centerEyeCursor.transform.position = gaze_target_world_position;
        centerEyeCursor.transform.localScale = Vector3.one * CalculateScaleForConstantVisualSize(gaze_target_distance, 1f, cursorRefScale);

        // ================= //
        // === Head Data === //
        // ================= //

        // 1. Initialize head data for recording
        Vector3 head_position = centerCamera.transform.position;
        Vector3 head_direction = centerCamera.transform.forward;
        Vector3 head_target_world_position = head_position + head_direction * gazeDistance;
        float head_target_distance = gazeDistance;
        string head_target_name = "";

        // 2. Use a raycast to detect a hit
        RaycastHit head_hit;
        if (Physics.Raycast(head_position, head_direction, out head_hit, gazeDistance, layersToInclude)) {
            head_target_world_position = head_hit.point;
            head_target_distance = head_hit.distance;
            head_target_name = head_hit.transform.gameObject.name;
        }

        // Calculate screen positions
        /*
        Vector3 gaze_target_screen_position = centerCamera.WorldToScreenPoint(gaze_target_world_position);
        Vector3 head_target_screen_position = centerCamera.WorldToScreenPoint(head_target_world_position);
        */
        Vector3 gaze_target_screen_position = leftCamera.WorldToScreenPoint(gaze_target_world_position);
        Vector3 head_target_screen_position = leftCamera.WorldToScreenPoint(head_target_world_position);

        // Calculate gaze vs head specifics
        Vector3 gaze_head_rel_direction = centerCamera.transform.InverseTransformDirection(gaze_direction);
        float gaze_head_angle_diff = Vector3.Angle(gaze_head_rel_direction, Vector3.forward);

        // Center Eye Record
        RecordEvent(
            // Event Logistics
            gaze_event,
            // Gaze Data
            gaze_target_world_position, gaze_target_screen_position, gaze_direction, gaze_target_distance, gaze_target_name,
            // Head Data
            head_target_world_position, head_target_screen_position, head_direction, head_position, head_target_distance, head_target_name, 
            // Gaze vs Head
            gaze_head_rel_direction, gaze_head_angle_diff
        );
    }

    public float GetCurrentTime() {
        return Time.time - startTime;
    }

    public void SetEyeCursorVisibility(bool set_to) {
        centerEyeCursor.enabled = set_to;
    }
    public void ToggleEyeCursorVisibility() {
        centerEyeCursor.enabled = !centerEyeCursor.enabled;
    }

    public float CalculateScaleFromAngularSize(float a) {
        // tan(a) = opp/adj. If adj==1, then tan(a)=opp
        // Therefore, scale = 2f * tan(a)
        return 2f*Mathf.Tan((a*Mathf.Deg2Rad)/2f);
    }

    public float CalculateScaleForConstantVisualSize(
        float distance,     // Distance from source
        float refDistance,  // 1 meter...
        float refScale      // At refDistance, what should the size be
    ) {
        return refScale * (distance / refDistance);
    }

    public static Vector3 CalculateNormAvgVector(Vector3 v1, Vector3 v2, bool normalized=true) {
        Vector3 avg = (v1+v2)/2f;
        return (normalized) ? avg.normalized : avg;
    }

    

    /*
    [System.Serializable]
    public struct Trial
    {
        public string scene_name;
        public bool static_rotation;
    }

    [Header("=== References ===")]
    public Transform head_ref;
    public Transform cal_sphere_ref;
    public Follower cal_follower_ref;
    public Transform gaze_target_ref;
    public AudioSource gaze_audiosrc_ref;
    public Transform head_cursor_ref;
    public Transform left_cursor_ref;
    public Transform right_cursor_ref;
    public Renderer head_cursor_renderer_ref;
    public Renderer left_cursor_renderer_ref;
    public Renderer right_cursor_renderer_ref;
    public GameObject calibration_textbox_ref;
    public GameObject free_textbox_ref;
    public GameObject restricted_textbox_ref;


    [Header("=== Settings ===")]
    public float target_angular_size = 2.3f;
    public float cursor_angular_size = 1f;
    public Trial[] trials;
    public bool randomize_trials;


    [Header("=== Outcomes (Read-Only) ===")]
    public Trial[] reshuffled_trials;
    public int current_trial_index = 0;
    public Vector3 head_cursor_local_pos = Vector3.zero;

    public CSVWriter writer;

    private void Awake()
    {
        // Initialization
        Instance = this;
        reshuffled_trials = Reshuffle<Trial>(trials, true);
        current_trial_index = -1;
        Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(90f);
        writer.Initialize();

        // Mod the cursor and target scales
        gaze_target_ref.localScale = Vector3.one * CalculateScaleFromAngularSize(target_angular_size);
        head_cursor_ref.localScale = Vector3.one * CalculateScaleFromAngularSize(cursor_angular_size);
        left_cursor_ref.localScale = Vector3.one * CalculateScaleFromAngularSize(cursor_angular_size);
        right_cursor_ref.localScale = Vector3.one * CalculateScaleFromAngularSize(cursor_angular_size);

        // First: calibration write + timestamp
        writer.AddPayload(-1);
        writer.AddPayload("Calibration");
        writer.WriteLine(true);

    }

    // The only thing we do at the start is allow the user to calibrate their head rotation forward.
    // The idea is that at the beginning, the `Calibration Sphere` is always hwading true forward
    // So we need to calibrate the head cursor to consider that as the primary rotation.
    // We do this by rotating the calibration sphere by an offset.
    public void CalibrateHeadForward()
    {
        cal_follower_ref.CalculateRotationOffset();
        head_cursor_ref.localPosition = head_ref.InverseTransformDirection(gaze_target_ref.position - head_ref.position).normalized;
    }

    // Trials will actually be added additively, to ensure that we don't lose any references to calibrations, etc.
    // When the `NextTrial()` button is pressed, we need to do several things:
    // 1. Unload the previously added trial
    // 2. Increment our trial index
    // 2a. If our next trial index is beyond the list of trials, then we're done; we close up shop.
    // 3. Load in the new additive scene
    // 4. Modify `Eye Session`'s components with the proper references
    // 5. Start the trial.
    public void NextTrial()
    {
        // If the calibration step wasn't completed (which we can check with its `completed` boolean), then initialize calibration
        // Otherwise, continue with loading the next scene.
        if (Calibrator.Instance != null && !Calibrator.Instance.completed)
        {
            Calibrator.Instance.Play();
            return;
        }

        // 1,2,3,4 are all covered below.
        current_trial_index += 1;
        if (current_trial_index >= reshuffled_trials.Length) {
            // End the scene!
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
        else LoadScene();
    }

    // Unity is a bit bonkers because, while in editor mode, additively adding and unloading scenes is blocked
    // The only way to do this is to use events attached to SceneManagement, to fix the unloadsceneasync problem too.
    private void LoadScene()
    {
        SceneManager.sceneLoaded += ActivatorAndUnloader;
        SceneManager.LoadScene(reshuffled_trials[current_trial_index].scene_name, LoadSceneMode.Additive);
    }
    private void ActivatorAndUnloader(Scene scene, LoadSceneMode mode) {
        SceneManager.sceneLoaded -= ActivatorAndUnloader;
        SceneManager.SetActiveScene(scene);
        InitializeScene();
        if (current_trial_index - 1 >= 0) SceneManager.UnloadSceneAsync(reshuffled_trials[current_trial_index - 1].scene_name);
    }


    public void InitializeScene()
    {
        // Write to our writer what our current trial is.
        writer.AddPayload(current_trial_index);
        writer.AddPayload(reshuffled_trials[current_trial_index].scene_name);
        writer.WriteLine(true);

        // Let the EyeSession know what trial we are on.
        // This operations also lets EyeSession get references to our own references.
        EyeSession.Instance.Initialize(
            reshuffled_trials[current_trial_index].scene_name,
            writer.dirName
        );

        // Let the Calibrator initialize its list of steps.
        // This operation also lets Calibrator get references to our own references.
        Calibrator.Instance.Initialize();

        // Force the follower to rotate or not
        cal_follower_ref.follow_rotation = reshuffled_trials[current_trial_index].static_rotation;

        // We don't force the calibrator to play JUST yet. We want the user to wait until they are prepared.
        // They should be able to start the next trial upon clicking the Start button again.
            ToggleCursorRenderers(true);
            gaze_target_ref.localPosition = Vector3.forward;
    }

    public void ToggleCursorRenderers(bool set_to)
    {
        head_cursor_renderer_ref.enabled = set_to;
        left_cursor_renderer_ref.enabled = set_to;
        right_cursor_renderer_ref.enabled = set_to;
    }

    private T[] Reshuffle<T>(T[] original, bool randomize = true)
    {
        // Copy the array to prevent mutation. Copy only from the start to the end, exclusive
        T[] outcome = new T[original.Length];
        int k = 0;
        for (int i = 0; i < original.Length; i++)
        {
            outcome[k] = original[i];
            k++;
        }

        // Iterate through the outcome to randomize, if needed
        if (randomize)
        {
            for (int i = 0; i < outcome.Length; i++)
            {
                T tmp = outcome[i];
                int r = Random.Range(i, outcome.Length);
                outcome[i] = outcome[r];
                outcome[r] = tmp;
            }
        }

        // Return the randomized subarray
        return outcome;
    }

    public float CalculateScaleFromAngularSize(float a) {
        // tan(a) = opp/adj. If adj==1, then tan(a)=opp
        // Therefore, scale = 2f * tan(a)
        return 2f*Mathf.Tan((a*Mathf.Deg2Rad)/2f);
    }

    void OnApplicationQuit()
    {
        writer.Disable();
    }
    */
}
