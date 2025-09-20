#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using VTBeat.Asset;

namespace VTBeat.Extensions {
    public static class AssetDatabaseX {
        public static bool TryFindFirstPrefab<TAssetRef, TAssetType>(out TAssetRef assetRef)
            where TAssetRef : AssetReferenceComponent<TAssetType>, new()
            where TAssetType : Component {
            //
            assetRef = null;
            
            GUID[] guids = AssetDatabase.FindAssetGUIDs($"t:Prefab");
            foreach (GUID guid in guids) {
                var prefab = AssetDatabase.LoadAssetByGUID<GameObject>(guid);
                if (prefab.TryGetComponent(out TAssetType _)) {
                    assetRef = new TAssetRef();
                    assetRef.SetEditorAsset(prefab);
                    return true;
                }
            }
            return false;
        }
        public static bool TryFindFirstAsset<TAssetRef, TAssetType>(out TAssetRef assetRef)
            where TAssetRef : AssetReferenceUObject<TAssetType>, new()
            where TAssetType : UObject {
            //
            assetRef = null;
            
            GUID[] guids = AssetDatabase.FindAssetGUIDs($"t:{typeof(TAssetType)}");
            if (guids.Length > 0) {
                assetRef = new TAssetRef();
                assetRef.SetEditorAsset(AssetDatabase.LoadAssetByGUID<TAssetType>(guids[0]));
                return true;
            }
            return false;
        }
    }
}
#endif