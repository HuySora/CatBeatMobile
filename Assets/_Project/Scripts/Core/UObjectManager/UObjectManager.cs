using UnityEngine;
using UnityEngine.AddressableAssets;
using UObject = UnityEngine.Object;

namespace VTBeat {
    // TODO: Pooling, TryCreate<T> where T : UObject
    public class UObjectManager : MonoBehaviour {
        public bool TryCreate<T>(AssetReference assetRef, out T instance) where T : MonoBehaviour {
            instance = null;
            if (assetRef == null) {
                Debug.LogWarning("assetRef is null");
                return false;
            }
            if (!assetRef.RuntimeKeyIsValid()) {
                Debug.LogWarning("assetRef.RuntimeKey is invalid");
                return false;
            }
            
            return TryCreate(assetRef.RuntimeKey.ToString(), out instance);
        }
        public bool TryCreate<T>(string key, out T instance) where T : MonoBehaviour {
            Debug.Log($"Create instance with key={key.RTColorByHash()}");
            instance = null;
            if (!SL.TryGet(out AssetManager assetMgr)) return false;
            if (!assetMgr.TryGetAsset(key, out GameObject asset)) return false;
            
            Debug.Log($"Creating instance with key={key.RTColorByHash()}");
            var gObj = UObject.Instantiate(asset);
            
            Debug.Log($"Created instance with key={key.RTColorByHash()}");
            instance = gObj.GetComponent<T>();
            return true;
        }
    }
}