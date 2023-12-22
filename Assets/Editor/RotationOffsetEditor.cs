// Name this script "RotateAtPointEditor"
using UnityEngine;
using UnityEditor;
using ModelReplacement;
using UnityEngine.WSA;
using ModelReplacement.AvatarBodyUpdater;
using Codice.CM.Client.Differences;

[CustomEditor(typeof(RotationOffset))]
[CanEditMultipleObjects]
public class RotationOffsetEditor : Editor
{
    public void OnSceneGUI()
    {
        RotationOffset t = (target as RotationOffset);


        EditorGUI.BeginChangeCheck();
        Quaternion rot = Handles.RotationHandle(t.transform.rotation * t.offset, t.transform.position);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Rotated RotateAt Point");
            t.offset = Quaternion.Inverse(t.transform.rotation) * rot;
        }
    }

    void OnEnable()
    {
        //folder = (Transform)target;
        Tools.hidden = true;
    }

    void OnDisable()
    {
        Tools.hidden = false;
    }
}
