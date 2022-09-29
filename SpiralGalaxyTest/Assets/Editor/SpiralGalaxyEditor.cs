using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpiralGalaxy))]
public class SpiralGalaxyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUI.changed && Application.isPlaying)
        {
            Debug.Log("Changed");
            SpiralGalaxy galaxy = (SpiralGalaxy)target;
            galaxy.InitialiseOrbits();
        }
    }
}
