using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
#if UNITY_EDITOR // => Ignore from here to next endif if not in editor
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.XR;

namespace ModelReplacement.AvatarBodyUpdater
{

    [ExecuteInEditMode]
    [AddComponentMenu("Model Replacement Properties")]
    public class OffsetBuilder : MonoBehaviour
    {
        public Vector3 rootPositionOffset = new Vector3(0, 0, 0);
        [HideInInspector]
        public Vector3 rootScale = new Vector3(1, 1, 1);
        public Vector3 itemPositonOffset = new Vector3(0.002728224f, 0.009688641f, -0.05092087f);
        public Quaternion itemRotationOffset = Quaternion.Euler(328.5828f, 4.848449f, 350.3954f);
        [HideInInspector]
        public GameObject itemHolder;
        [HideInInspector]
        public Transform rootTransform;


#if UNITY_EDITOR // => Ignore from here to next endif if not in editor
        [HideInInspector]
        public GameObject playerObject;
        [HideInInspector]
        public GameObject item;
        [HideInInspector]
        public GameObject playerHumanoid;

        public bool renderPlayer = false;
        public bool renderItem = true;

        public string assetBundleName = "";
        public string assetName = "";



       
        private IEnumerator SaveAssetBundle()
        {
            if (assetBundleName == "")
            {
                Debug.LogError($"Asset Bundle Name must be set");
                yield break; 
            }
            if (assetName == "")
            {
                Debug.LogError($"Asset Name must be set");
                yield break; 
            }
            yield return null;
            if (!Directory.Exists("Assets/ModelReplacementAPI/AssetsToBuild"))
            {
                Directory.CreateDirectory("Assets/ModelReplacementAPI/AssetsToBuild");
            }
            string AssetPath = "Assets/ModelReplacementAPI/AssetsToBuild/" + assetName + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(gameObject, AssetPath);
            yield return null;

            AssetImporter assetImporter = AssetImporter.GetAtPath(AssetPath);
            assetImporter.assetBundleName = assetBundleName;

            string assetBundleDirectory = "Assets/AssetBundles";
            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }
            BuildPipeline.BuildAssetBundles(assetBundleDirectory,BuildAssetBundleOptions.None,BuildTarget.StandaloneWindows);
            EditorUtility.RevealInFinder(assetBundleDirectory + "/" + assetBundleName);
        }

        private void OnValidate()
        {
            //CalculateScale();

        }

        public void SaveAssetBundles()
        {
            StartCoroutine(SaveAssetBundle());
        }

        public Animator animator;
        public bool hasUpperChest = false;
        private Bounds GetBounds()
        {
            Bounds bounds = new Bounds();
            var allBounds = base.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().Select(r => r.bounds);

            float maxX = allBounds.OrderByDescending(x => x.max.x).First().max.x;
            float maxY = allBounds.OrderByDescending(x => x.max.y).First().max.y;
            float maxZ = allBounds.OrderByDescending(x => x.max.z).First().max.z;

            float minX = allBounds.OrderBy(x => x.min.x).First().min.x;
            float minY = allBounds.OrderBy(x => x.min.y).First().min.y;
            float minZ = allBounds.OrderBy(x => x.min.z).First().min.z;

            bounds.SetMinMax(new Vector3(minX, minY, minZ), new Vector3(maxZ, maxY, maxZ));
            return bounds;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (playerObject != null) { return; }
            UnityEngine.Object pPrefab = AssetDatabase.LoadAssetAtPath("Assets/ModelReplacementAPI/Player.prefab", typeof(GameObject)); 
            UnityEngine.Object wPrefab = AssetDatabase.LoadAssetAtPath("Assets/ModelReplacementAPI/WalkieTalkie.prefab", typeof(GameObject));
            UnityEngine.Object hPrefab = AssetDatabase.LoadAssetAtPath("Assets/ModelReplacementAPI/Scavenger.prefab", typeof(GameObject));
            playerObject = (GameObject)PrefabUtility.InstantiatePrefab(pPrefab);
            item = (GameObject)PrefabUtility.InstantiatePrefab(wPrefab);
            playerHumanoid = (GameObject)PrefabUtility.InstantiatePrefab(hPrefab);
            playerObject.name = playerObject.name + $"({base.name})";
            item.name = item.name + $"({base.name})";
            playerHumanoid.name = playerHumanoid.name + $"({base.name})";

            assetName = base.name;

            animator = base.GetComponentInChildren<Animator>();
            Transform upperChestTransform = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            hasUpperChest = (upperChestTransform != null);

            rootTransform = GetPlayerTransformFromBoneName("spine");

            var rht = animator.GetBoneTransform(HumanBodyBones.RightHand);
            for (int i = 0; i < rht.childCount; i++)
            {
                Transform cht = rht.GetChild(i);
                if (cht.name == "ItemHolderTransform")
                {
                    itemHolder = cht.gameObject;
                }
            }
            if(itemHolder == null)
            {
                var tempgo = new GameObject("ItemHolderTransform");
                itemHolder = GameObject.Instantiate(tempgo, rht);
                DestroyImmediate(tempgo);
                itemHolder.name = "ItemHolderTransform";
                itemHolder.transform.parent = rht;
            }


            playerHumanoid.GetComponentInChildren<Animator>().avatar.humanDescription.skeleton.ToList().ForEach(sk =>
            {
                var a = playerHumanoid.GetComponentsInChildren<Transform>().Where(x => x.name == sk.name);
                if (a.Any())
                {
                    a.First().localRotation = sk.rotation;
                }
                else
                {
                    Debug.Log($"Missing bone {sk.name}");
                }

            });
            animator.avatar.humanDescription.skeleton.ToList().ForEach(sk =>
            {
                var a = base.GetComponentsInChildren<Transform>().Where(x => x.name == sk.name);
                if (a.Any())
                {
                    a.First().localRotation = sk.rotation;
                }
                else
                {
                    Debug.Log($"Missing bone {sk.name}");
                }
            });
            var playerBodyExtents = playerObject.GetComponentInChildren<SkinnedMeshRenderer>().bounds.extents;
            float scale = playerBodyExtents.y / GetBounds().extents.y;
            base.transform.localScale *= scale;
           // PopulateFingers();
            baseScale = base.transform.localScale;
            CalculateScale();
            CalculateRotationOffsets();
            CalculateRootOffset();



        }
        /*
        public void PopulateFingers()
        {
            if(fingerDrivers.Count != 0) { return; }
            fingerDrivers = new List<FingerDriver>();
            fingerDrivers.Add(new FingerDriver(GetPlayerTransformFromBoneName("finger5.L"), GetPlayerTransformFromBoneName("finger5.L.001"),
                animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal), animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate), animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal)));
            fingerDrivers.Add(new FingerDriver(GetPlayerTransformFromBoneName("finger4.L"), GetPlayerTransformFromBoneName("finger4.L.001"),
                animator.GetBoneTransform(HumanBodyBones.LeftRingProximal), animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate), animator.GetBoneTransform(HumanBodyBones.LeftRingDistal)));
            fingerDrivers.Add(new FingerDriver(GetPlayerTransformFromBoneName("finger3.L"), GetPlayerTransformFromBoneName("finger3.L.001"),
                animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal), animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate), animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal)));
            fingerDrivers.Add(new FingerDriver(GetPlayerTransformFromBoneName("finger2.L"), GetPlayerTransformFromBoneName("finger2.L.001"),
                animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal), animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate), animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal)));
            fingerDrivers.Add(new FingerDriver(GetPlayerTransformFromBoneName("finger1.L"), GetPlayerTransformFromBoneName("finger1.L.001"),
                animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal), animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate), animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal)));

            fingerDrivers.Add(new FingerDriver(GetPlayerTransformFromBoneName("finger5.R"), GetPlayerTransformFromBoneName("finger5.R.001"),
                animator.GetBoneTransform(HumanBodyBones.RightLittleProximal), animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate), animator.GetBoneTransform(HumanBodyBones.RightLittleDistal)));
            fingerDrivers.Add(new FingerDriver(GetPlayerTransformFromBoneName("finger4.R"), GetPlayerTransformFromBoneName("finger4.R.001"),
                animator.GetBoneTransform(HumanBodyBones.RightRingProximal), animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate), animator.GetBoneTransform(HumanBodyBones.RightRingDistal)));
            fingerDrivers.Add(new FingerDriver(GetPlayerTransformFromBoneName("finger3.R"), GetPlayerTransformFromBoneName("finger3.R.001"),
                animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal), animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate), animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal)));
            fingerDrivers.Add(new FingerDriver(GetPlayerTransformFromBoneName("finger2.R"), GetPlayerTransformFromBoneName("finger2.R.001"),
                animator.GetBoneTransform(HumanBodyBones.RightIndexProximal), animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate), animator.GetBoneTransform(HumanBodyBones.RightIndexDistal)));
            fingerDrivers.Add(new FingerDriver(GetPlayerTransformFromBoneName("finger1.R"), GetPlayerTransformFromBoneName("finger1.R.001"),
                animator.GetBoneTransform(HumanBodyBones.RightThumbProximal), animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate), animator.GetBoneTransform(HumanBodyBones.RightThumbDistal)));
        }
        */

        public Vector3 baseScale = Vector3.zero;
        public static List<string> DontCalculateOffset = new List<string>()
        {
                "finger5.L" ,
                "finger5.L.001" ,
                "finger4.L" ,
                "finger4.L.001" ,
                "finger3.L" ,
                "finger3.L.001" ,
                "finger2.L" ,
                "finger2.L.001" ,
                "finger1.L" ,
                "finger1.L.001" ,
                "finger5.R" ,
                "finger5.R.001" ,
                "finger4.R" ,
                "finger4.R.001" ,
                "finger3.R" ,
                "finger3.R.001" ,
                "finger2.R" ,
                "finger2.R.001" ,
                "finger1.R" ,
                "finger1.R.001" ,

        };

        public void CalculateScale()
        {
            if(playerObject == null) return;
            base.transform.localScale = ((new Vector3(1, 0, 0)) * baseScale.x * rootScale.x + (new Vector3(0, 1, 0)) * baseScale.y * rootScale.y + (new Vector3(0, 0, 1)) * baseScale.z * rootScale.z);
        }

        public void CalculateRotationOffsets()
        {
            foreach (Transform playerBone in playerObject.GetComponentInChildren<SkinnedMeshRenderer>().bones)
            {
                Transform modelBone = GetAvatarTransformFromBoneName(playerBone.name);
                if (modelBone == null) { continue; }



                Transform humanBone = null;
                var someBones = playerHumanoid.GetComponentsInChildren<Transform>().Where(x => x.name == playerBone.name.Replace(".", "_"));
                if (someBones.Any()) { humanBone = someBones.First(); }



                if (!modelBone.gameObject.GetComponent<RotationOffset>())
                {
                    var a = modelBone.gameObject.AddComponent<RotationOffset>();

                    if (humanBone )
                    {
                        a.offset = Quaternion.Inverse(playerBone.rotation) * modelBone.rotation;

                    }
                }

            }
        }
        public void CalculateRootOffset()
        {
            //Translation offset done after all rotation offsets are done
            foreach (Transform playerBone in playerObject.GetComponentInChildren<SkinnedMeshRenderer>().bones)
            {
                Transform modelBone = GetAvatarTransformFromBoneName(playerBone.name);
                if (modelBone == null) { continue; }

                if (playerBone.name == "spine")
                {
                    Vector3 playerfoot = playerHumanoid.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.LeftToes).position;
                    Vector3 modelFoot = GetAvatarLowestTransform().position;
                    Vector3 diff = playerfoot - modelFoot;
                    diff.x = 0f;
                    diff.y *= -1;
                    rootPositionOffset = playerBone.InverseTransformVector(diff);
                    //rootPositionOffset = diff;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (playerObject == null) { return; }
           // PopulateFingers();
            playerObject.GetComponentInChildren<SkinnedMeshRenderer>().enabled = renderPlayer;
            item.GetComponentInChildren<Renderer>().enabled = renderItem;
            CalculateScale();
            // playerObject.GetComponentsInChildren<Renderer>().ToList().ForEach(r => r.enabled = false);
            animator = base.GetComponentInChildren<Animator>();
            Debug.Log($"Bones {playerObject.GetComponentInChildren<SkinnedMeshRenderer>().bones.Count()}");


            itemHolder.transform.localPosition = itemPositonOffset;


            item.transform.rotation = itemHolder.transform.rotation * itemRotationOffset;
            item.transform.Rotate(new Vector3(30.01f, 5.9f, 12.54f));
            item.transform.position = itemHolder.transform.position;
            Vector3 vector = new Vector3(-0.012f, 0.085f, 0.01f) * 50;
            vector = itemHolder.transform.rotation * vector;
            item.transform.position += vector;

            foreach (Transform playerBone in playerObject.GetComponentInChildren<SkinnedMeshRenderer>().bones)
            {
                Transform modelBone = GetAvatarTransformFromBoneName(playerBone.name);
                if (modelBone == null) { continue; }
                modelBone.rotation = playerBone.rotation;
                var offset = modelBone.GetComponent<RotationOffset>();
                if (offset) { modelBone.rotation *= offset.offset; }

                if (playerBone.name == "spine")
                {
                    modelBone.position = playerBone.position;

                    modelBone.position += playerBone.TransformVector(rootPositionOffset);
                    //modelBone.position += rootPositionOffset;
                }
            }
        }

        private void OnDestroy()
        {
            DestroyImmediate(item);
            DestroyImmediate(playerObject);
            DestroyImmediate(playerHumanoid);
        }


        private Transform GetAvatarLowestTransform()
        {
            if (animator.GetBoneTransform(HumanBodyBones.LeftToes))
            {
                return animator.GetBoneTransform(HumanBodyBones.LeftToes);
            }
            if (animator.GetBoneTransform(HumanBodyBones.LeftFoot))
            {
                return animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            }
            if (animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg))
            {
                return animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            }
            if (animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg))
            {
                return animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            }
            return animator.GetBoneTransform(HumanBodyBones.Hips);
        }
        private Transform GetAvatarTransformFromBoneName(string boneName)
        {
            //Special logic is required here. The player model has 5 central bones.
            // Spine, spine.001, spine.002, spine.003,   spine.004, corresponding to 
            // Hips   Spine      Chest      UpperChest   Head
            //However spine.002 practically doesn't move, and I wish to support mods that don't have an UpperChest bone. 
            //If they don't have an upperchest bone, instead map spine.003 to the Chest transform on the replacement model.
            if (boneName == "spine.002")
            {
                if (hasUpperChest) { return animator.GetBoneTransform(HumanBodyBones.Chest); }
                else { return null; }
            }
            if (boneName == "spine.003")
            {
                if (hasUpperChest) { return animator.GetBoneTransform(HumanBodyBones.UpperChest); }
                else { return animator.GetBoneTransform(HumanBodyBones.Chest); }
            }
            if (modelToAvatarBone.ContainsKey(boneName))
            {
                return animator.GetBoneTransform(modelToAvatarBone[boneName]);
            }
            return null;
        }

        public Transform GetPlayerTransformFromBoneName(string boneName)
        {
            var a = playerObject.GetComponentInChildren<SkinnedMeshRenderer>().bones.Where(x => x.name == boneName);
            if (a.Any()) { return a.First(); }
            if (boneName == "spine")
            {
                var b = playerObject.GetComponentInChildren<SkinnedMeshRenderer>().bones.Where(x => x.name.Contains("PlayerRagdoll")); //For ragdoll and etc...
                if (b.Any()) { return b.First(); }
            }
            return null;

        }

        

        //Remove spine.002 and .003 to implement logic
        public static Dictionary<string, HumanBodyBones> modelToAvatarBone = new Dictionary<string, HumanBodyBones>()
            {
                {"spine" , HumanBodyBones.Hips},
                {"spine.001" , HumanBodyBones.Spine},

                // {"spine.002" , HumanBodyBones.Chest},
                //{"spine.003" , HumanBodyBones.UpperChest},

                {"shoulder.L" , HumanBodyBones.LeftShoulder},
                {"arm.L_upper" , HumanBodyBones.LeftUpperArm},
                {"arm.L_lower" , HumanBodyBones.LeftLowerArm},
                {"hand.L" , HumanBodyBones.LeftHand},
                {"finger5.L" , HumanBodyBones.LeftLittleProximal},
                {"finger5.L.001" , HumanBodyBones.LeftLittleIntermediate},
                {"finger4.L" , HumanBodyBones.LeftRingProximal},
                {"finger4.L.001" , HumanBodyBones.LeftRingIntermediate},
                {"finger3.L" , HumanBodyBones.LeftMiddleProximal},
                {"finger3.L.001" , HumanBodyBones.LeftMiddleIntermediate},
                {"finger2.L" , HumanBodyBones.LeftIndexProximal},
                {"finger2.L.001" , HumanBodyBones.LeftIndexIntermediate},
                {"finger1.L" , HumanBodyBones.LeftThumbProximal},
                {"finger1.L.001" , HumanBodyBones.LeftThumbDistal},

                {"shoulder.R" , HumanBodyBones.RightShoulder},
                {"arm.R_upper" , HumanBodyBones.RightUpperArm},
                {"arm.R_lower" , HumanBodyBones.RightLowerArm},
                {"hand.R" , HumanBodyBones.RightHand},
                {"finger5.R" , HumanBodyBones.RightLittleProximal},
                {"finger5.R.001" , HumanBodyBones.RightLittleIntermediate},
                {"finger4.R" , HumanBodyBones.RightRingProximal},
                {"finger4.R.001" , HumanBodyBones.RightRingIntermediate},
                {"finger3.R" , HumanBodyBones.RightMiddleProximal},
                {"finger3.R.001" , HumanBodyBones.RightMiddleIntermediate},
                {"finger2.R" , HumanBodyBones.RightIndexProximal},
                {"finger2.R.001" , HumanBodyBones.RightIndexIntermediate},
                {"finger1.R" , HumanBodyBones.RightThumbProximal},
                {"finger1.R.001" , HumanBodyBones.RightThumbDistal},

                {"spine.004" , HumanBodyBones.Head},

                {"thigh.L" , HumanBodyBones.LeftUpperLeg},
                {"shin.L" , HumanBodyBones.LeftLowerLeg},
                {"foot.L" , HumanBodyBones.LeftFoot},
                {"toe.L" , HumanBodyBones.LeftToes},

                {"thigh.R" , HumanBodyBones.RightUpperLeg},
                {"shin.R" , HumanBodyBones.RightLowerLeg},
                {"foot.R" , HumanBodyBones.RightFoot},
                {"toe.R" , HumanBodyBones.RightToes},
        };

#endif
    }

}


