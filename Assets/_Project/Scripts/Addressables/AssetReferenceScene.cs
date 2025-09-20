using System;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VTBeat.Asset {
    [Serializable]
    public class AssetReferenceScene : AssetReference {
        public AssetReferenceScene() : base() { }
        public AssetReferenceScene(string guid) : base(guid) { }
        
#if UNITY_EDITOR
        public new SceneAsset editorAsset => base.editorAsset as SceneAsset;
#endif
        
        public override bool ValidateAsset(UObject obj) {
#if UNITY_EDITOR
            return typeof(SceneAsset).IsAssignableFrom(obj.GetType());
#else
            return false;
#endif
        }
        public override bool ValidateAsset(string path) {
#if UNITY_EDITOR
            return typeof(SceneAsset).IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(path));
#else
            return false;
#endif
        }
    }
}