using UnityEngine;
using UnityEditor;

namespace ReplaySet
{
    [CustomEditor(typeof(Replay))]
    public class ReplayEditor : Editor
    {

        Replay replay;

        void OnEnable()
        {
            replay = (Replay)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Load Trial"))
            {
                replay.LoadFiles();
            }

            if (replay.trials.Length > 0)
            {
                foreach (Trial t in replay.trials)
                {
                    if (GUILayout.Button($"Play Trial {t.trial_index + 1}"))
                    {
                        replay.PlayTrial(t);
                    }
                }
            }
        }
    }
}