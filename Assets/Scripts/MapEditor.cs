using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Map))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Map map = (Map)target;

        if (GUILayout.Button("Generate"))
        {
            map.Generate();
        }

        if (GUILayout.Button("Initialise Zones"))
        {
            map.InitialiseZones();
        }
    }
}
