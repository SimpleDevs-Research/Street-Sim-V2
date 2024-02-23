using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestSaveAnimation))]
public class TestSaveAnimationEditor : Editor
{
    /*
    public override void OnInspectorGUI() {
        TestSaveAnimation manager = (TestSaveAnimation)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Save State")) {
            manager.RecordState();
        }

        if (GUILayout.Button("Load State")) {
            manager.RestoreState();
        }
    }
    */
}
