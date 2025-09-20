using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MackySoft.Navigathena;
using MackySoft.Navigathena.SceneManagement;
using MackySoft.Navigathena.Transitions;
using UnityEngine;
using UnityEngine.SceneManagement;
using VTBeat.Stage.Core;

namespace VTBeat.Stage {
    // TODO: Asynchronous request cause problems
    public class StageManager {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void HandleBeforeSceneLoad() {
            GlobalSceneNavigator.Instance.Register(new StageNavigator());
        }
        
        // PREF: Not sure if it inlined
        private ISceneNavigator m_SceneNavigator => GlobalSceneNavigator.Instance;
        private bool m_Initialized;
        
        public async UniTask Initialize(EmptyStage emptyStage, CancellationToken ct = default) {
            if (m_Initialized) return;
            m_Initialized = true;
            
            try {
                await m_SceneNavigator.Push(
                    emptyStage.GetSceneIdentifier(),
                    null,
                    emptyStage.GetSceneData(),
                    emptyStage.GetBeforeSceneLoadAsyncOperation(),
                    ct
                );
            }
            catch {
                m_Initialized = false;
                throw;
            }
        }
        
        #region Load
        public async UniTask<Scene> LoadAsync(StageAsset stage, ITransitionDirector transition = null, CancellationToken ct = default) =>
            await LoadAsync(
                stage.GetSceneIdentifier(),
                transition,
                stage.GetSceneData(),
                stage.GetBeforeSceneLoadAsyncOperation(),
                ct
            );
        private async UniTask<Scene> LoadAsync(ISceneIdentifier sceneId, ITransitionDirector transition, ISceneData data, IAsyncOperation op, CancellationToken ct = default) {
            await m_SceneNavigator.Replace(sceneId, transition, data, op, ct);
            return SceneManager.GetActiveScene();
        }
        #endregion
        
        public async UniTask WaitUntilUnloadAsync(string sceneName, CancellationToken ct = default) {
            // TODO: Wait until the scene (StageAsset) finish all it unloading method
            while (SceneManager.GetSceneByName(sceneName).isLoaded) {
                ct.ThrowIfCancellationRequested();
                await UniTask.Yield();
            }
        }
    }
}