using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraFollow))]
[CanEditMultipleObjects]
public class CameraFollowEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CameraFollow script = (CameraFollow)target;
        if (GUILayout.Button("Align camera"))
        {
            script.ResetPosition();
        }
    }
}