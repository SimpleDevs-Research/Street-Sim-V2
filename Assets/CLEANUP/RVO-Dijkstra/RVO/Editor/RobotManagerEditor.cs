using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RobotManager))]
[CanEditMultipleObjects]
public class RobotManagerEditor : Editor 
{
    RobotManager manager;

    public override void OnInspectorGUI()
    {
        manager = (RobotManager)target;
        
        DrawDefaultInspector();
        if(GUILayout.Button("Create Ring")) manager.CreateDirections();
    }
}
