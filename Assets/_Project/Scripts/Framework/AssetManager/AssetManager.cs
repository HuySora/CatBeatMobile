using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VTBeat.Extensions;
using static VTBeat.LoggerAssetManager;

namespace VTBeat.Asset {
    public class AssetManager {
        private readonly ConcurrentDictionary<string, LoadedAsset> m_Key2LoadedAsset = new();
        
        public class LoadedAsset {
            public AsyncOperationHandle Handle;
            public int RefCount;
        }
        
        #region GetAsset
        public bool TryGetAsset<T>(AssetReference assetRef, out T asset) where T : UObject {
            Log.ZLogDebug($"[{nameof(TryGetAsset)}] Request: assetRef?.Key={assetRef?.RuntimeKey?.ToString().RTColorByHash() ?? "null"}, assetRef?.RuntimeKeyIsValid()={assetRef?.RuntimeKeyIsValid()}");
            asset = null;
            if (assetRef == null) return false;
            if (!assetRef.RuntimeKeyIsValid()) return false;
            
            return TryGetAsset(assetRef.RuntimeKey!.ToString(), out asset);
        }
        public bool TryGetAsset<T>(string key, out T asset) where T : UObject {
#if UNITY_EDITOR
            string keyFormatted = key.ToAddressablesEditorKey();
#else
            string keyFormatted = key.RTColorByHash();
#endif
            Log.ZLogDebug($"[{nameof(TryGetAsset)}] Request: key={keyFormatted}");
            asset = null;
            if (!m_Key2LoadedAsset.TryGetValue(key, out var loadedAsset)) return false;
            if (!loadedAsset.Handle.IsDone) return false;
            if (!loadedAsset.Handle.IsValid()) return false;
            
            loadedAsset.RefCount += 1;
            Log.ZLogInformation($"[{nameof(TryGetAsset)}] Existed: key={keyFormatted}, refCount={loadedAsset.RefCount}");
            asset = loadedAsset.Handle.Result as T;
            return true;
        }
        #endregion
        
        #region Acquire
        // TODO: This should return list of assets just for coding convention
        public async UniTask AcquireAsync(IEnumerable<AssetReference> assetRefs, CancellationToken ct = default) {
            var countStr = assetRefs == null
                ? "null"
                : assetRefs is ICollection<AssetReference> col
                    ? col.Count.ToString()
                    : assetRefs.Count().ToString();
            Log.ZLogDebug($"[{nameof(AcquireAsync)}] Request: assetRefs.Count={countStr}");
            if (assetRefs == null) return;
            var assetRefList = assetRefs.ToList();
            if (assetRefList.Count == 0) return;
            
            // TODO: Parallel
            foreach (var assetRef in assetRefList) {
                await AcquireAsync<UObject>(assetRef, ct);
            }
        }
        public async UniTask<T> AcquireAsync<T>(AssetReference assetRef, CancellationToken ct = default) where T : UObject {
            Log.ZLogDebug($"[{nameof(AcquireAsync)}] Request: assetRef?.Key={assetRef?.RuntimeKey?.ToString().RTColorByHash() ?? "null"}, assetRef?.RuntimeKeyIsValid()={assetRef?.RuntimeKeyIsValid()}");
            if (assetRef == null) return null;
            if (!assetRef.RuntimeKeyIsValid()) return null;
            
            return await AcquireAsync<T>(assetRef.RuntimeKey!.ToString(), ct);
        }
        
        public async UniTask<T> AcquireAsync<T>(string key, CancellationToken ct = default) where T : UObject {
#if UNITY_EDITOR
            string keyFormatted = key.ToAddressablesEditorKey();
#else
            string keyFormatted = key.RTColorByHash();
#endif
            Log.ZLogTrace($"[{nameof(AcquireAsync)}] Starting: key={keyFormatted}");
            if (!m_Key2LoadedAsset.TryGetValue(key, out var loadedAsset)) {
                // First time loading this
                Log.ZLogTrace($"[{nameof(AcquireAsync)}] Loading: key={keyFormatted}");
                loadedAsset = new LoadedAsset {
                    Handle = Addressables.LoadAssetAsync<UObject>(key),
                    RefCount = 0
                };
                m_Key2LoadedAsset[key] = loadedAsset;
            }
            
            loadedAsset.RefCount += 1;
            Log.ZLogTrace($"[{nameof(AcquireAsync)}] RefCount+: key={keyFormatted}, refCount={loadedAsset.RefCount}");
            try {
                await loadedAsset.Handle.WithCancellation(ct);
            }
            catch (Exception ex) {
                Release(key);
                Log.ZLogError($"[{nameof(AcquireAsync)}] Exception: key={keyFormatted}, refCount={loadedAsset.RefCount}, ex={ex}");
                throw;
            }
            
            if (loadedAsset.Handle.Status == AsyncOperationStatus.Succeeded) {
                Log.ZLogInformation($"[{nameof(AcquireAsync)}] Loaded: key={keyFormatted}, refCount={loadedAsset.RefCount}");
                
                var result = loadedAsset.Handle.Result;
                if (result is GameObject prefab) {
                    Log.ZLogTrace($"[{nameof(AcquireAsync)}] Loaded from prefab: key={keyFormatted}, refCount={loadedAsset.RefCount}");
                    return prefab.GetComponent<T>();
                }
                return loadedAsset.Handle.Result as T;
            }
            
            Release(key);
            Log.ZLogError($"[{nameof(AcquireAsync)}] Failed: key={keyFormatted}, refCount={loadedAsset.RefCount}");
            return null;
        }
        #endregion
        
        #region Release
        public void Release(IEnumerable<AssetReference> assetRefs) {
            var countStr = assetRefs == null
                ? "null"
                : assetRefs is ICollection<AssetReference> col
                    ? col.Count.ToString()
                    : assetRefs.Count().ToString();
            Log.ZLogDebug($"[{nameof(Release)}] Request: assetRefs.Count={countStr}");
            if (assetRefs == null) return;
            var assetRefList = assetRefs.ToList();
            if (assetRefList.Count == 0) return;
            
            // TODO: Parallel
            foreach (var assetRef in assetRefList) {
                Release(assetRef);
            }
        }
        public void Release(AssetReference assetRef) {
            Log.ZLogDebug($"[{nameof(Release)}] Request: assetRef?.Key={assetRef?.RuntimeKey?.ToString().RTColorByHash() ?? "null"}, assetRef?.RuntimeKeyIsValid()={assetRef?.RuntimeKeyIsValid()}");
            if (assetRef == null) return;
            if (!assetRef.RuntimeKeyIsValid()) return;
            
            Release(assetRef.RuntimeKey.ToString());
        }
        
        public void Release(string key) {
#if UNITY_EDITOR
            string keyFormatted = key.ToAddressablesEditorKey();
#else
            string keyFormatted = key.RTColorByHash();
#endif
            Log.ZLogTrace($"[{nameof(Release)}] Starting: key={keyFormatted}");
            if (!m_Key2LoadedAsset.TryGetValue(key, out var loadedAsset)) {
                Log.ZLogWarning($"[{nameof(Release)}] Non-existed: key={keyFormatted}");
                return;
            }
            
            loadedAsset.RefCount -= 1;
            Log.ZLogTrace($"[{nameof(Release)}] RefCount-: key={keyFormatted}, refCount={loadedAsset.RefCount}");
            if (loadedAsset.RefCount > 0) return;
            
            Log.ZLogInformation($"[{nameof(Release)}] Unloading: key={keyFormatted}, refCount={loadedAsset.RefCount}");
            m_Key2LoadedAsset.TryRemove(key, out _);
            // This can't happen tbh
            if (!loadedAsset.Handle.IsValid()) return;
            
            Addressables.Release(loadedAsset.Handle);
        }
        #endregion
    }
}