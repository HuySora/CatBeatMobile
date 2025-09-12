using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UObject = UnityEngine.Object;

namespace VTBeat {
    public class AssetManager {
        public class LoadedAsset {
            public AsyncOperationHandle Handle;
            public int RefCount;
        }
        
        // We only want to use 1 handle per key to reduce GC
        private readonly ConcurrentDictionary<string, LoadedAsset> m_Key2LoadedAsset = new();
        
        #region GetAsset
        public bool TryGetAsset<T>(string key, out T asset) where T : UObject {
            asset = null;
            if (!m_Key2LoadedAsset.TryGetValue(key, out var loadedAsset)) return false;
            if (!loadedAsset.Handle.IsDone) return false;
            if (!loadedAsset.Handle.IsValid()) return false;
            
            asset = loadedAsset.Handle.Result as T;
            return true;
        }
        #endregion
        
        #region LoadAsset
        public async UniTask LoadAssetsAsync(IEnumerable<AssetReference> assetRefs) {
            if (assetRefs == null) return;
            var assetRefList = assetRefs.ToList();
            if (assetRefList.Count == 0) return;
            
            // TODO: Parallel
            foreach (var assetRef in assetRefList) {
                await LoadAssetAsync<UObject>(assetRef);
            }
        }
        public async UniTask<T> LoadAssetAsync<T>(AssetReference assetRef) where T : UObject {
            if (assetRef == null) {
                Debug.LogWarning("assetRef is null");
                return null;
            }
            if (!assetRef.RuntimeKeyIsValid()) {
                Debug.LogWarning("assetRef.RuntimeKey is invalid");
                return null;
            }
            
            return await LoadAssetAsync<T>(assetRef.RuntimeKey.ToString());
        }
        
        public async UniTask<T> LoadAssetAsync<T>(string key) where T : UObject {
            Debug.Log($"Load asset with key={key.RTColorByHash()}");
            if (!m_Key2LoadedAsset.TryGetValue(key, out var loadedAsset)) {
                // First time loading this
                Debug.Log($"Loading asset with key={key.RTColorByHash()}");
                loadedAsset = new LoadedAsset() {
                    Handle = Addressables.LoadAssetAsync<T>(key),
                    RefCount = 0
                };
                m_Key2LoadedAsset[key] = loadedAsset;
            }
            loadedAsset.RefCount += 1;
            await loadedAsset.Handle.ToUniTask();
            
            if (loadedAsset.Handle.Status == AsyncOperationStatus.Succeeded) {
                Debug.Log($"Loaded asset with key={key.RTColorByHash()}, refCount={loadedAsset.RefCount}");
                return loadedAsset.Handle.Result as T;
            }
            else {
                UnloadAsset(key);
                Debug.LogError($"Failed to load asset: " +
                               $"key={key}, " +
                               $"status={loadedAsset.Handle.Status}, " +
                               $"debugName={loadedAsset.Handle.DebugName}, " +
                               $"error={loadedAsset.Handle.OperationException}"
                );
                return null;
            }
        }
        #endregion
        
        #region UnloadAsset
        public void UnloadAssets(IEnumerable<AssetReference> assetRefs) {
            if (assetRefs == null) return;
            var assetRefList = assetRefs.ToList();
            if (assetRefList.Count == 0) return;
            
            // TODO: Parallel
            foreach (var assetRef in assetRefList) {
                UnloadAsset(assetRef);
            }
        }
        public void UnloadAsset(AssetReference assetRef) {
            if (assetRef == null) {
                Debug.LogWarning("assetRef is null");
                return;
            }
            if (!assetRef.RuntimeKeyIsValid()) {
                Debug.LogWarning("assetRef.RuntimeKey is invalid");
                return;
            }
            
            UnloadAsset(assetRef.RuntimeKey.ToString());
        }
        
        public void UnloadAsset(string key) {
            Debug.Log($"Unloading asset with key={key.RTColorByHash()}");
            if (!m_Key2LoadedAsset.TryGetValue(key, out var loadedAsset)) {
                return;
            }
            
            loadedAsset.RefCount -= 1;
            Debug.Log($"Unload asset with key={key.RTColorByHash()}, refCount={loadedAsset.RefCount}");
            if (loadedAsset.RefCount > 0) return;
            
            if (!loadedAsset.Handle.IsValid()) return;
            Debug.Log($"Unloaded asset with key={key.RTColorByHash()}, refCount={loadedAsset.RefCount}");
            Addressables.Release(loadedAsset.Handle);
        }
        #endregion
    }
}