using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UObject = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VTBeat {
    [Serializable]
    public class AssetReferenceSceneSwitchButton : AssetReference {
        public AssetReferenceSceneSwitchButton(string guid) : base(guid) { }
#if UNITY_EDITOR
        public new GameObject editorAsset => base.editorAsset as GameObject;
#endif
        
        public override bool ValidateAsset(UObject obj) {
#if UNITY_EDITOR
            var gObj = obj as GameObject;
            return gObj != null && gObj.GetComponent<SceneSwitchButton>() != null;
#else
        return false;
#endif
        }
        
        public override bool ValidateAsset(string path) {
#if UNITY_EDITOR
            var gObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return gObj != null && gObj.GetComponent<SceneSwitchButton>() != null;
#else
        return false;
#endif
        }
    }
}