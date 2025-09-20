#if UNITY_EDITOR
using VTBeat.Asset;

namespace VTBeat.Extensions {
    public static class AssetReferenceX {
        public static bool TryConvert<TAssetType, TAsAssetType>(this AssetReferenceUObject<TAssetType> assetRef, out AssetReferenceUObject<TAsAssetType> asAssetRef)
            where TAssetType : TAsAssetType
            where TAsAssetType : UObject {
            //
            asAssetRef = null;
            
            if (assetRef.editorAsset == null) {
                return false;
            }
            if (assetRef.editorAsset is not TAsAssetType type) {
                return false;
            }
            
            asAssetRef = new AssetReferenceUObject<TAsAssetType>();
            asAssetRef.SetEditorAsset(type);
            
            return false;
        }
    }
}
#endif