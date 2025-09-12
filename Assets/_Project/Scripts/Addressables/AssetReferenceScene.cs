using System;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VTBeat {
    [Serializable]
    public class AssetReferenceScene : AssetReference {
        public AssetReferenceScene(string guid) : base(guid) { }
#if UNITY_EDITOR
        public new SceneAsset editorAsset => (SceneAsset)base.editorAsset;
#endif
        
        public override bool ValidateAsset(Object obj) {
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