using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
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
#endif
