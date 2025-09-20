using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VTBeat.Asset {
    [Serializable]
    public class AssetReferenceComponent<T> : AssetReference where T : Component {
        public AssetReferenceComponent() : base() { }
        public AssetReferenceComponent(string guid) : base(guid) { }
        
        public new T Asset {
            get {
                if (m_Asset != null) return m_Asset;
                
                m_Asset = (base.Asset as GameObject)?.GetComponent<T>();
                return m_Asset;
            }
        }
        private T m_Asset;
        
#if UNITY_EDITOR
        public new GameObject editorAsset => base.editorAsset as GameObject;
#endif
        
        public override bool ValidateAsset(UObject obj) {
#if UNITY_EDITOR
            var gObj = obj as GameObject;
            return gObj != null && gObj.GetComponent<T>() != null;
#else
        return false;
#endif
        }
        
        public override bool ValidateAsset(string path) {
#if UNITY_EDITOR
            var gObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return gObj != null && gObj.GetComponent<T>() != null;
#else
        return false;
#endif
        }
    }
}