using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MackySoft.Navigathena;
using MackySoft.Navigathena.SceneManagement;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using VTBeat.Asset;
using VTBeat.View;
using static VTBeat.LoggerCoreStage;

namespace VTBeat.Stage.Core {
    [DefaultExecutionOrder(-40)]
    public partial class EmptyContext : MonoBehaviour {
        [field: SerializeField, FoldoutGroup("Runtime")] public EmptyStage m_EmptyStage;
        private void Awake() {
            Log.ZLogTrace($"[{nameof(EmptyContext)}][{nameof(Awake)}]");
        }
        private void OnEnable() {
            Log.ZLogTrace($"[{nameof(EmptyContext)}][{nameof(OnEnable)}]");
        }
        private void Start() {
            Log.ZLogTrace($"[{nameof(EmptyContext)}][{nameof(Start)}]");
        }
        private void OnDisable() {
            Log.ZLogTrace($"[{nameof(EmptyContext)}][{nameof(OnDisable)}]");
        }
        private void OnDestroy() {
            Log.ZLogTrace($"[{nameof(EmptyContext)}][{nameof(OnDestroy)}]");
        }
    }
    
    public partial class EmptyContext : ISceneEntryPoint {
        public UniTask OnInitialize(ISceneDataReader reader, IProgress<IProgressDataStore> prog, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(EmptyContext)}][{nameof(OnInitialize)}]");
            SL.TryGet(out AssetManager assetMgr);
            reader.TryRead(out EmptyStage emptyStage);
            m_EmptyStage = emptyStage;
            
            return UniTask.CompletedTask;
        }
        public UniTask OnEnter(ISceneDataReader reader, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(EmptyContext)}][{nameof(OnEnter)}]");
            return UniTask.CompletedTask;
        }
        public UniTask OnExit(ISceneDataWriter writer, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(EmptyContext)}][{nameof(OnExit)}]");
            return UniTask.CompletedTask;
        }
        public async UniTask OnFinalize(ISceneDataWriter writer, IProgress<IProgressDataStore> prog, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(EmptyContext)}][{nameof(OnFinalize)}]");
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            
            // TODO: Remove fake loading
            var store = new ProgressDataStore<LoadingProgressData>();
            for (int i = 0; i < 9; i++) {
                float percent = i / 10f;
                prog.Report(store.SetData(new LoadingProgressData(ProgressCycle.PreviousFinalize, percent, "EmptyStage.OnFinalize")));
                await UniTask.WaitForSeconds(0.04f, cancellationToken: cts.Token);
            }
            
            await m_EmptyStage.OnBeforeSceneUnloadedAsyncOperationAsync(prog, cts.Token);
        }
#if UNITY_EDITOR
        public UniTask OnEditorFirstPreInitialize(ISceneDataWriter writer, CancellationToken ct) => UniTask.CompletedTask;
#endif
    }
}