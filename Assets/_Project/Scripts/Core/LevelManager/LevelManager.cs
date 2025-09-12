using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace VTBeat {
    public class LevelManager {
        public class LevelProperties {
            public LevelAsset Level;
            public Scene Scene;
            public bool IsPersistent;
        }
        // Scene loading can only happen on main-thread so we don't need concurrent collections
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> m_Key2Handles = new();
        private readonly Dictionary<Scene, LevelProperties> m_Scene2LevelProperties = new();
        
        public void AddToPersistent(Scene scene) {
            if (!m_Scene2LevelProperties.TryGetValue(scene, out LevelProperties props)) return;
            
            props.IsPersistent = true;
        }
        public void RemoveFromPersistent(Scene scene) {
            if (!m_Scene2LevelProperties.TryGetValue(scene, out LevelProperties props)) return;
            
            props.IsPersistent = false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetActiveScene(Scene scene) => SceneManager.SetActiveScene(scene);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scene GetActiveScene() => SceneManager.GetActiveScene();
        
        #region LoadScene
        public async UniTask<Scene> LoadSceneAsync(LevelAsset levelAsset, LoadSceneMode mode = LoadSceneMode.Single, bool activeOnLoad = true) {
            if (levelAsset == null) return default;
            
            return await LoadSceneAsync(levelAsset.SceneAssetReference, mode, activeOnLoad);
        }
        private async UniTask<Scene> LoadSceneAsync(AssetReferenceScene sceneRef, LoadSceneMode mode = LoadSceneMode.Single, bool activeOnLoad = true) {
            if (sceneRef == null) return default;
            
            return await LoadSceneAsync(sceneRef.RuntimeKey.ToString(), mode, activeOnLoad);
        }
        private async UniTask<Scene> LoadSceneAsync(string key, LoadSceneMode mode = LoadSceneMode.Single, bool activeOnLoad = true) {
            Debug.Log($"LoadSceneAsync: key={key.RTColorByHash()}");
            switch (mode) {
                case LoadSceneMode.Single:
                    return await LoadSceneSingleAsync(key, activeOnLoad);
                case LoadSceneMode.Additive:
                    return await LoadSceneAdditiveAsync(key, activeOnLoad);
                default:
                    return default;
            }
        }
        private async UniTask<Scene> LoadSceneSingleAsync(string key, bool activeOnLoad = true) {
            // Already loaded
            if (m_Key2Handles.TryGetValue(key, out var hdl)) {
                await hdl.ToUniTask();
            }
            
            await UnloadAllScene();
            return await LoadSceneAdditiveAsync(key, activeOnLoad);
        }
        private async UniTask<Scene> LoadSceneAdditiveAsync(string key, bool activeOnLoad = true) {
            // First load
            if (!m_Key2Handles.TryGetValue(key, out var hdl)) {
                hdl = Addressables.LoadSceneAsync(key, LoadSceneMode.Additive, activeOnLoad);
                m_Key2Handles[key] = hdl;
            }
            await hdl.ToUniTask();
            
            UniTask<SceneInstance> uTask = hdl.ToUniTask();
            
            if (hdl.Status == AsyncOperationStatus.Succeeded) {
                // Previous op could load the scene with activeOnLoad=false
                if (activeOnLoad) {
                    await hdl.Result.ActivateAsync();
                }
                
                return hdl.Result.Scene;
            }
            m_Key2Handles.Remove(key);
            Debug.LogError($"Failed to load scene: {key}");
            return default;
        }
        #endregion
        
        #region UnloadScene
        public async UniTask UnloadAllScene() {
            List<AsyncOperationHandle<SceneInstance>> hdlList = m_Key2Handles.Select(kv => kv.Value).ToList();
            await UniTask.WhenAll(hdlList.Select(hdl => hdl.ToUniTask()));
            
            // TODO: Do it parallel
            List<Scene> keyList = m_Scene2LevelProperties.Keys.ToList();
            foreach (var scene in keyList) {
                var props =  m_Scene2LevelProperties[scene];
                
                if (props.IsPersistent) continue;
                
                await UnloadSceneAsync(scene);
                m_Scene2LevelProperties.Remove(scene);
            }
        }
        
        // private async UniTask UnloadSceneAsync(AssetReferenceScene sceneRef) {
        //     if (sceneRef == null) return;
        //     
        //     string key = sceneRef.RuntimeKey.ToString();
        //     await UnloadSceneAsync(key);
        // }
        // private async UniTask UnloadSceneAsync(string key) {
        //     // Already unloaded
        //     if (!m_Key2Handles.TryGetValue(key, out var hdl)) {
        //         // TODO: Check for non-Addressables scene
        //         return;
        //     }
        //     await hdl.ToUniTask();
        //     
        //     if (hdl.Status == AsyncOperationStatus.Failed) {
        //         return;
        //     }
        //     
        //     await UnloadSceneAsync(hdl.Result.Scene);
        //     
        //     Addressables.Release(hdl);
        //     m_Key2Handles.Remove(key);
        // }
        private async UniTask UnloadSceneAsync(Scene scene) {
            await SceneManager.UnloadSceneAsync(scene);
            await WaitUntilUnloadAsync(scene.name);
            if (!m_Scene2LevelProperties.TryGetValue(scene, out LevelProperties props)) return;
            await props.Level.OnAfterSceneUnloadAsync();
        }
        #endregion
        
        #region WaitUntilUnload
        public async UniTask WaitUntilUnloadAsync(string sceneName) {
            // TODO: Wait until LevelAsset.OnAfterSceneUnloadAsync()
            while (SceneManager.GetSceneByName(sceneName).isLoaded) {
                await UniTask.Yield();
            }
        }
        #endregion
    }
}