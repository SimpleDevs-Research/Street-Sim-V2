using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ReplaySet
{

    [System.Serializable]
    public struct NameToTransformRef
    {
        public string obj_name;
        public Transform transform_ref;
        public Vector3 orig_position;
        public Quaternion orig_rotation;
    }

    [System.Serializable]
    public class Trial
    {
        public int trial_index;
        public StreetSimCarManager.CarManagerStatus car_manager_status;
        public long start_unix;
        public float start_timestamp;
        public int start_frame;
        public long end_unix;
        public float end_timestamp;
        public int end_frame;

        [Space]
        public long duration_unix;
        public float duration_time;
        public int duration_frame;
        public List<Position>[] positions_by_frame;
        [HideInInspector] public List<Eye> eyes;

        public Trial(string serialized, string col_divider = ",")
        {
            // Basics
            string[] values = serialized.Split(col_divider, StringSplitOptions.None);
            this.trial_index = int.Parse(values[0]);
            this.car_manager_status = (StreetSimCarManager.CarManagerStatus)System.Enum.Parse(typeof(StreetSimCarManager.CarManagerStatus), values[1]);
            this.start_unix = long.Parse(values[2]);
            this.start_timestamp = float.Parse(values[3]);
            this.start_frame = int.Parse(values[4]);
            this.end_unix = long.Parse(values[5]);
            this.end_timestamp = float.Parse(values[6]);
            this.end_frame = int.Parse(values[7]);
            // Advanced
            this.duration_unix = end_unix - start_unix;
            this.duration_time = end_timestamp - start_timestamp;
            this.duration_frame = end_frame - start_frame;
            this.positions_by_frame = new List<Position>[duration_frame];
            for (int i = 0; i < duration_frame; i++) this.positions_by_frame[i] = new List<Position>();
            this.eyes = new List<Eye>();
        }
    }

    [System.Serializable]
    public class Position
    {
        public long unix_ms;
        public float rel_timestamp;
        public int frame;
        public int guid;
        public string obj_name;
        public Vector3 position;
        public Vector3 forward;

        [Space]
        public Transform transform_ref;
        public int replay_frame;
        public Position(string serialized, string col_divider = ",")
        {
            // We ONLY do basics for now.
            string[] values = serialized.Split(col_divider, StringSplitOptions.None);
            this.unix_ms = long.Parse(values[0]);
            this.rel_timestamp = float.Parse(values[1]);
            this.frame = int.Parse(values[2]);
            this.obj_name = values[3];
            this.guid = int.Parse(values[4]);
            this.position = new Vector3(float.Parse(values[5]), float.Parse(values[6]), float.Parse(values[7]));
            this.forward = new Vector3(float.Parse(values[8]), float.Parse(values[9]), float.Parse(values[10]));
        }
    }

    [System.Serializable]
    public class Eye
    {
        public long unix_ms;
        public float rel_timestamp;
        public int frame;
        public string evnt;
        public string side;
        public Vector3 screen_position;
        public string target_name;

        [Space]
        public int trial_index;
        public int replay_frame;
        public Vector3 local_direction;
        public float angular_diff;
        [HideInInspector] public string[] values;

        public Eye(string serialized, string col_divider = ",")
        {
            // Base only
            values = serialized.Split(col_divider, StringSplitOptions.None);
            this.unix_ms = long.Parse(values[0]);
            this.rel_timestamp = float.Parse(values[1]);
            this.frame = int.Parse(values[2]);
            this.evnt = values[3].Trim();
            this.side = values[4].Trim();
            this.screen_position = new Vector3(float.Parse(values[5]), float.Parse(values[6]), float.Parse(values[7]));
            this.target_name = values[8];
            this.local_direction = new Vector3(float.Parse(values[9]), float.Parse(values[10]), float.Parse(values[11])).normalized;
            this.angular_diff = float.Parse(values[12]);
        }

        public string UpdateCalculations(Camera cam_ref, Transform gaze_ref, LayerMask eye_raycast_targets)
        {
            // We assume that the position of the camera is updating
            //Vector3 ray_direction = world_position - cam_ref.transform.position;

            Vector3 ray_direction = cam_ref.transform.TransformDirection(this.local_direction);
            Debug.DrawRay(cam_ref.transform.position, ray_direction, Color.cyan);

            RaycastHit hit;
            if (Physics.Raycast(cam_ref.transform.position, ray_direction, out hit, Mathf.Infinity, eye_raycast_targets))
            {
                gaze_ref.position = hit.point;
                return hit.transform.gameObject.name;
            }
            return "";
        }
    }

    public class Replay : MonoBehaviour
    {
        [HideInInspector] public static Replay Instance;

        [Header("=== Files ===")]
        public TextAsset trial_file;
        public TextAsset positions_file;
        public TextAsset eye_file;

        [Header("=== Replay Settings ===")]
        public bool load_trials_on_start = true;
        public bool play_trials_on_start = true;
        public Camera center_eye_ref;
        public LayerMask eye_raycast_targets;
        public List<NameToTransformRef> manual_transform_refs;
        public Dictionary<string, NameToTransformRef> transforms_dict = new Dictionary<string, NameToTransformRef>();
        public Transform gaze_ref;
        public List<ReplayPositionNotifier> position_extraction_targets;

        [Header("=== Loaded Data ===")]
        public string[] trial_col_names;
        public Trial[] trials;

        [Space]
        public string[] positions_col_names;
        [ReadOnlyInsp] public int unknown_positions = 0;

        [Space]
        public string[] eyes_col_names;
        [ReadOnlyInsp] public int unknown_eyes = 0;

        [Space]
        public CSVWriter writer;
        public CSVWriter moddedPositionWriter;

        public void Awake()
        {
            Instance = this;
            position_extraction_targets = new List<ReplayPositionNotifier>();
        }
        public void Start()
        {
            if (load_trials_on_start) LoadFiles();
            if (play_trials_on_start) PlayAllTrials();

        }

        public void LoadFiles()
        {
            // Read the Trial file, which returns all trials for the associated user.
            string[] trial_raw = ReadCSVFile(trial_file, out trial_col_names, out int num_trials, out int num_trial_cols);
            trials = new Trial[num_trials];
            for (int i = 0; i < num_trials; i++)
            {
                if (trial_raw[i].Length > 0) trials[i] = new Trial(trial_raw[i]);
            }

            // Initialize `transform_dict` from `manual_transform_refs` if any matches are detected
            transforms_dict = new Dictionary<string, NameToTransformRef>();
            if (manual_transform_refs.Count > 0)
            {
                foreach (NameToTransformRef _ref in manual_transform_refs)
                {
                    if (transforms_dict.ContainsKey(_ref.obj_name))
                    {
                        NameToTransformRef tr = transforms_dict[_ref.obj_name];
                        tr.transform_ref = _ref.transform_ref;
                        transforms_dict[_ref.obj_name] = tr;
                    }
                    else
                    {
                        transforms_dict.Add(_ref.obj_name, new NameToTransformRef()
                        {
                            obj_name = _ref.obj_name,
                            transform_ref = _ref.transform_ref,
                            orig_position = _ref.transform_ref.position,
                            orig_rotation = _ref.transform_ref.rotation
                        });
                    }
                }
            }

            // Read the positions file to get the positions of all relative gameobjects in each trial
            string[] positions_raw = ReadCSVFile(positions_file, out positions_col_names, out int num_positions_samples, out int num_positions_cols);
            unknown_positions = 0;
            for (int i = 0; i < num_positions_samples; i++)
            {
                if (positions_raw[i].Length == 0) continue;
                Position p = new Position(positions_raw[i]);            // Create a new position
                Transform t = GetOrAddTransformByName(p.obj_name);      // Check if this is associated with a transform
                if (t == null)
                {
                    unknown_positions += 1;
                    continue;
                }
                p.transform_ref = t;
                bool trial_found = false;                               // Have to check if this position's frame matches with any of the trials.
                foreach (Trial trial in trials)
                {
                    if (p.frame >= trial.start_frame && p.frame < trial.end_frame)
                    {
                        p.replay_frame = p.frame - trial.start_frame;
                        trial.positions_by_frame[p.replay_frame].Add(p);
                        trial_found = true;
                        break;
                    }
                }
                if (!trial_found) unknown_positions += 1;
            }

            // Read the eye data file to get the screen positions of each eye
            string[] eyes_raw = ReadCSVFile(eye_file, out eyes_col_names, out int num_eye_samples, out int num_eyes_cols);
            unknown_eyes = 0;
            // We skip the first true row because it's empty
            for (int i = 1; i < num_eye_samples; i++)
            {
                if (eyes_raw[i].Length == 0) continue;
                Eye e = new Eye(eyes_raw[i]);
                // Only keep the eye referencing the center eye
                if (e.side != "Center") continue;
                // Compare with other trials. Add to eye data if trial found
                bool trial_found = false;
                foreach (Trial trial in trials)
                {
                    if (e.frame >= trial.start_frame && e.frame < trial.end_frame)
                    {
                        e.replay_frame = e.frame - trial.start_frame;
                        e.trial_index = trial.trial_index;
                        trial.eyes.Add(e);
                        trial_found = true;
                        break;
                    }
                }
                if (!trial_found) unknown_eyes += 1;
            }
        }

        public void PlayAllTrials()
        {
            StartCoroutine(PlayAllTrialsCoroutine());
        }

        public IEnumerator PlayAllTrialsCoroutine()
        {
            // We're outputting all eye data to a new output file
            // This time, we initialize the writer.
            writer.Initialize();
            moddedPositionWriter.Initialize();

            string[] positions_raw = ReadCSVFile(positions_file, out positions_col_names, out int num_positions_samples, out int num_positions_cols);
            int pos_raw_ind = 0;

            // We need to loop through each trial, producing a continuous stream
            for (int i = 0; i < trials.Length; i++)
            {
                Trial trial = trials[i];
                foreach (Eye e in trial.eyes)
                {
                    int frame = e.replay_frame;
                    ResetPositions();

                    string lastLineCache = ""; //Bandaid fix for entities being written out of order -- don't know what's up with that. 

                    string[] ref_line = positions_raw[pos_raw_ind].Split(",", StringSplitOptions.None);
                    for(int ii = 0; ii < trial.positions_by_frame[frame].Count; ii++)
                    {
                        // Try to find the reference to this object in transform_dict

                        Position p = trial.positions_by_frame[frame][ii];
                        p.transform_ref.position = p.position;
                        p.transform_ref.rotation = Quaternion.LookRotation(p.forward);

                        if (ii != trial.positions_by_frame[frame].Count - 1)
                        {
                            // Copy the line over from the original data
                            moddedPositionWriter.WriteLine(positions_raw[pos_raw_ind]);
                        }
                        else
                        {
                            lastLineCache = positions_raw[pos_raw_ind];
                        }
                        pos_raw_ind++;

                    }

                    //foreach (Position p in trial.positions_by_frame[frame])

                    // Add new data from replayed objects
                    foreach (ReplayPositionNotifier rpn in position_extraction_targets)
                    {
                        UpdatePosition(ref_line[0], ref_line[1], ref_line[2], rpn._name, rpn._guid, rpn.transform.position, rpn.transform.forward);
                    }
                    moddedPositionWriter.WriteLine(lastLineCache);

                    // Update our writer
                    foreach (string v in e.values) writer.AddPayload(v);
                    writer.AddPayload(e.UpdateCalculations(center_eye_ref, gaze_ref, eye_raycast_targets));
                    writer.WriteLine(false);
                    // Let the next frame run
                    yield return null;
                }
            }

            // Upon termination, disable the writer and reset all positions
            writer.Disable();
            ResetPositions();
            // End the scene!
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        void OnDestroy()
        {
            writer.Disable();
        }
        

        public void PlayTrial(Trial trial)
        {
            // For each trial, we must iterate through the trial, looking at each frame of the eye data, 
            // and attempt to identify what the object of attention really is.

            if (trial.eyes.Count == 0)
            {
                Debug.LogError($"Cannot play trial {trial.trial_index}: No eye data loaded");
                return;
            }
            if (trial.positions_by_frame.Length == 0)
            {
                Debug.LogError($"Cannot play trial {trial.trial_index}: No positions loaded");
                return;
            }

            if (Application.isPlaying)
            {
                StartCoroutine(PlayTrialLive(trial));
            }
            else if (Application.isEditor)
            {
                foreach (Eye e in trial.eyes)
                {
                    int frame = e.replay_frame;
                    ResetPositions();
                    foreach (Position p in trial.positions_by_frame[frame])
                    {
                        // Try to find the reference to this object in transform_dict
                        p.transform_ref.position = p.position;
                        p.transform_ref.rotation = Quaternion.LookRotation(p.forward);
                    }
                    e.UpdateCalculations(center_eye_ref, gaze_ref, eye_raycast_targets);
                }
            }
        }

        public IEnumerator PlayTrialLive(Trial trial)
        {
            string[] positions_raw = ReadCSVFile(positions_file, out positions_col_names, out int num_positions_samples, out int num_positions_cols);
            foreach (Eye e in trial.eyes)
            {
                int frame = e.replay_frame;
                ResetPositions();
                foreach (Position p in trial.positions_by_frame[frame])
                {
                    // Try to find the reference to this object in transform_dict
                    p.transform_ref.position = p.position;
                    p.transform_ref.rotation = Quaternion.LookRotation(p.forward);
                }

                Debug.Log(positions_raw[frame]);
                moddedPositionWriter.AddPayload(positions_raw[frame]);
                moddedPositionWriter.WriteLine();

                e.UpdateCalculations(center_eye_ref, gaze_ref, eye_raycast_targets);
                yield return null;
            }
        }

        public void ResetPositions()
        {
            foreach (NameToTransformRef tr in manual_transform_refs) {
                tr.transform_ref.position = tr.orig_position;
                tr.transform_ref.rotation = tr.orig_rotation;
            }
        }

        public void ResetTrials()
        {
            trials = null;
        }

        public Transform GetOrAddTransformByName(string obj_name)
        {
            // Check if our transform dictionary has this or not.
            if (transforms_dict.ContainsKey(obj_name)) return transforms_dict[obj_name].transform_ref;
            // If not, we search for it in the hierarchy.
            GameObject go = GameObject.Find(obj_name);
            if (go != null)
            {
                // Add to dictionary
                transforms_dict.Add(obj_name, new NameToTransformRef()
                {
                    obj_name = obj_name,
                    transform_ref = go.transform,
                    orig_position = go.transform.position,
                    orig_rotation = go.transform.rotation
                });
                // Add to our list
                manual_transform_refs.Add(new NameToTransformRef()
                {
                    obj_name = obj_name,
                    transform_ref = go.transform,
                    orig_position = go.transform.position,
                    orig_rotation = go.transform.rotation
                });
                // Return appropriate
                return go.transform;
            }
            // Object could not be found. Return null.
            Debug.LogError($"Could not find Transform object with name {obj_name}");
            return null;
        }

        public static string[] ReadCSVFile(TextAsset ta, out string[] header, out int num_rows, out int num_cols, string colDivider = ",", string rowDivider = "\n")
        {
            // Get all rows, including the header row
            string[] rows_raw = ta.text.Split(rowDivider, StringSplitOptions.None);

            // Interpret header, get both column names and # of columns + rows
            header = rows_raw[0].Split(colDivider, StringSplitOptions.None);

            // Get only rows excluding header
            List<string> rows = new List<string>();
            for (int i = 1; i < rows_raw.Length; i++)
            {
                if (rows_raw[i].Length > 0) rows.Add(rows_raw[i]);
            }

            // Returnables
            num_cols = header.Length;
            num_rows = rows.Count;
            return rows.ToArray();
        }

        public static int GetSampleCount(string[] data, int numCols)
        {
            return data.Length / numCols - 1;
        }

        public void UpdatePosition(string unix_ms, string t, string frame, string _name, int _guid, Vector3 p, Vector3 f)
        {
            if (moddedPositionWriter.is_active && p.y > -30) //Brute force check to see if object is actually in the scene
            {
                moddedPositionWriter.AddPayload(unix_ms);
                moddedPositionWriter.AddPayload(t);
                moddedPositionWriter.AddPayload(frame);
                moddedPositionWriter.AddPayload(_name);
                moddedPositionWriter.AddPayload(_guid);
                moddedPositionWriter.AddPayload(p);
                moddedPositionWriter.AddPayload(f);
                moddedPositionWriter.WriteLine();
            }
        }
    }
}

