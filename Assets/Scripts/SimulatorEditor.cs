using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Simulator))]
public class SimulatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Simulator simulator = (Simulator)target;

        if (GUILayout.Button("Test"))
        {
            simulator.Test();
        }
    }
}
