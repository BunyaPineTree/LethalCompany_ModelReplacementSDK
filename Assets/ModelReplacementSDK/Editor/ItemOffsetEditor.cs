// Name this script "RotateAtPointEditor"
using UnityEngine;
using UnityEditor;
using ModelReplacement;
using ModelReplacement.AvatarBodyUpdater;
[CustomEditor(typeof(OffsetBuilder))]
[CanEditMultipleObjects]
public class OffsetBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        OffsetBuilder t = (OffsetBuilder)target;

        EditorGUILayout.LabelField("Base Model Properties");
        t.rootPositionOffset = EditorGUILayout.Vector3Field("Root Position Offset", t.rootPositionOffset);
        EditorGUILayout.Vector3Field("Root Scale", t.rootScale);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Item Transform Properties");
        t.itemPositonOffset = EditorGUILayout.Vector3Field("Item Position Offset", t.itemPositonOffset);
        t.itemRotationOffset.eulerAngles = EditorGUILayout.Vector3Field("Item Rotation Offset", t.itemRotationOffset.eulerAngles);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("AssetBundle Properties");
        t.assetBundleName = EditorGUILayout.TextField("Asset Bundle Name" ,t.assetBundleName);
        t.assetName = EditorGUILayout.TextField("Asset Name", t.assetName);
        if (GUILayout.Button("Build AssetBundle"))
        {
            t.SaveAssetBundles();
        }

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Controls");
        t.renderPlayer = EditorGUILayout.Toggle("Render Player",t.renderPlayer);
        t.renderItem = EditorGUILayout.Toggle("Render Item", t.renderItem);
        if (GUILayout.Button("ReZero Model Position"))
        {
            t.CalculateRootOffset();
        }


    }


    public void OnSceneGUI()
    {
        OffsetBuilder t = (target as OffsetBuilder);
        Transform handTransform = t.gameObject.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
        Transform itemTransform = t.item.transform;

        Vector3 rootOff = t.rootPositionOffset;
        Vector3 rootSca = t.rootScale;

        Vector3 itemOff = t.itemPositonOffset;
        Quaternion itemRot = t.itemRotationOffset;


        EditorGUI.BeginChangeCheck();
        if(Tools.current == Tool.Move)
        {
            rootOff = Handles.PositionHandle(t.animator.GetBoneTransform(HumanBodyBones.Hips).position, Quaternion.identity);
            itemOff = Handles.PositionHandle(t.itemHolder.transform.position, t.itemHolder.transform.rotation * itemRot);
        }
        if (Tools.current == Tool.Scale)
        {
             rootSca = Handles.ScaleHandle(rootSca, t.rootTransform.position, t.rootTransform.rotation);
        }
        if (Tools.current == Tool.Rotate)
        {
            itemRot = Handles.RotationHandle(t.itemHolder.transform.rotation * itemRot, t.itemHolder.transform.position);
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Rotated RotateAt Point");
            if (Tools.current == Tool.Move)
            {
                t.rootPositionOffset = t.rootTransform.InverseTransformVector(rootOff - t.animator.GetBoneTransform(HumanBodyBones.Hips).position) + t.rootPositionOffset;

                t.itemHolder.transform.SetPositionAndRotation(itemOff, t.itemHolder.transform.rotation);
                t.itemPositonOffset = t.itemHolder.transform.localPosition;
            }
            if (Tools.current == Tool.Scale)
            {
                t.rootScale = rootSca;
                
            }
            if (Tools.current == Tool.Rotate)
            {
                t.itemRotationOffset = Quaternion.Inverse(t.itemHolder.transform.rotation) * itemRot;
            }
            
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