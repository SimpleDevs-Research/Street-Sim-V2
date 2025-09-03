using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RVOSceneManager))]
[CanEditMultipleObjects]
public class RVOSceneManagerEditor : Editor 
{
    RVOSceneManager manager;

    public override void OnInspectorGUI()
    {
        manager = (RVOSceneManager)target;
        
        DrawDefaultInspector();
        if(GUILayout.Button("Set Grid Bounds")) manager.SetGridBounds();
        if(GUILayout.Button("Initialize Grid")) manager.InitializeGrid();
    }
}
