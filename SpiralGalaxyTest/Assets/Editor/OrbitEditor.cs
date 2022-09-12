using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Orbit))]
public class OrbitEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUI.changed)
        {
            Orbit creator = (Orbit)target;
            creator.DrawEllipse();
        }

    }
}

