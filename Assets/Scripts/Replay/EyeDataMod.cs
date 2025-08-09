using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ReplaySet
{
    public class EyeDataMod : MonoBehaviour
    {
        [System.Serializable]
        public struct EyeCollection
        {
            public string id;
            public TextAsset original_eye_data;
            public bool active;
            public CSVWriter writer;
        }

        public EyeCollection[] data;
        public Camera center_eye_ref;
        private bool processing = false;

        public void Start()
        {
            StartCoroutine(ProcessEyeData());
        }

        public IEnumerator ProcessEyeData()
        {
            // Iterate through our eye collection
            foreach (EyeCollection ec in data)
            {
                if (ec.active) yield return StartCoroutine(ReadWriteEye(ec));
            }
            // Quit application
            Application.Quit();
        }

        public IEnumerator ReadWriteEye(EyeCollection ec)
        {
            // Start the writer
            ec.writer.Initialize();

            // Read the EYE data
            string[] eyes_raw = Replay.ReadCSVFile(
                ec.original_eye_data,
                out string[] col_names,
                out int nrows,
                out int ncols
            );
            // We skip the first true row because it's empty
            for (int i = 1; i < nrows; i++)
            {
                // Skip any rows that are empty
                if (eyes_raw[i].Length == 0) continue;
                // Parse the row
                string[] values = eyes_raw[i].Split(",", StringSplitOptions.None);
                /*
                this.unix_ms = long.Parse(values[0]);
                this.rel_timestamp = float.Parse(values[1]);
                this.frame = int.Parse(values[2]);
                this.evnt = values[3].Trim();
                this.side = values[4].Trim();
                this.screen_position = new Vector3(float.Parse(values[5]), float.Parse(values[6]), float.Parse(values[7]));
                this.target_name = values[8];
                */
                // Only keep the eye referencing the center eye
                if (values[4] != "Center") continue;
                // Using the center eye camera, get the screen to world point, then local point
                Vector3 screen_position = new Vector3(float.Parse(values[5]), float.Parse(values[6]), float.Parse(values[7]));
                Vector3 local_position = center_eye_ref.transform.InverseTransformPoint(
                    center_eye_ref.ScreenToWorldPoint(screen_position)
                );
                // Calculate the angular diff
                float angular_diff = Vector3.Angle(local_position, Vector3.forward * screen_position.z);
                // Write original data
                ec.writer.AddPayload(values[0]);   // unix_ms
                ec.writer.AddPayload(values[1]);   // rel_timestamp
                ec.writer.AddPayload(values[2]);   // frame
                ec.writer.AddPayload(values[3]);   // event
                ec.writer.AddPayload(values[4]);   // side
                ec.writer.AddPayload(values[5]);   // screen_pos_x
                ec.writer.AddPayload(values[6]);   // screen_pos_y
                ec.writer.AddPayload(values[7]);   // screen_pos_z
                ec.writer.AddPayload(values[8]);   // target_name
                // Write new data
                ec.writer.AddPayload(local_position);  // local direction x,y,z
                ec.writer.AddPayload(angular_diff);    // Angular diff
                // Write
                ec.writer.WriteLine(false);
            }

            // Terminate writer
            ec.writer.Disable();
            // return
            yield return null;
        }
    }   
}
