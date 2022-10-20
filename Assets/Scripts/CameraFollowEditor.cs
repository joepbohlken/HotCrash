using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraController))]
[CanEditMultipleObjects]
public class CameraFollowEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CameraController script = (CameraController)target;
        if (GUILayout.Button("Align camera"))
        {
            script.ResetPosition();
        }
    }
}