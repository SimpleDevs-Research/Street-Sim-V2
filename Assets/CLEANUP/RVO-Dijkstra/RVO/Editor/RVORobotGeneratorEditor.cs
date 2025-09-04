using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RVORobotGenerator))]
[CanEditMultipleObjects]
public class RVORobotGeneratorEditor : Editor 
{
    RVORobotGenerator generator;

    public override void OnInspectorGUI()
    {
        generator = (RVORobotGenerator)target;
        
        DrawDefaultInspector();

        if(GUILayout.Button("Generate Robots")) generator.GenerateRobots();
        if(GUILayout.Button("Delete Robots")) generator.DeleteRobots();
    }
}
