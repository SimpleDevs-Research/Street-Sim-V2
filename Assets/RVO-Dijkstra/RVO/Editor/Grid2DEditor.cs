using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RVOGrid2D))]
[CanEditMultipleObjects]
public class Grid2DEditor : Editor 
{
    RVOGrid2D grid;

    public override void OnInspectorGUI()
    {
        grid = (RVOGrid2D)target;
        
        DrawDefaultInspector();
        if(GUILayout.Button("Generate Grid")) grid.GenerateGrid();
        if(GUILayout.Button("Reset Grid")) grid.ResetGrid();
    }
}
