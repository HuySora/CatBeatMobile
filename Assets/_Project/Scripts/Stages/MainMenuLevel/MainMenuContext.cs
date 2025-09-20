using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MackySoft.Navigathena;
using MackySoft.Navigathena.SceneManagement;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using VTBeat.Asset;
using VTBeat.Extensions;
using VTBeat.Stage.MainMenu;
using VTBeat.Stage.Standard;
using VTBeat.View;
using static VTBeat.LoggerMainMenuStage;

namespace VTBeat {
#if UNITY_EDITOR
    using UnityEditor;
    
    public partial class MainMenuContext : IPrepare {
        [FoldoutGroup("Runtime", false)]
        private bool m_EditorDummyBool;
        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton), PropertyOrder(-1)]
        public bool Prepare() {
            bool isDirty = false;
            return isDirty;
        }
    }
#endif
    
    [DefaultExecutionOrder(-40)]
    public partial class MainMenuContext : MonoBehaviour {
        [field: SerializeField, FoldoutGroup("Runtime")] public MainMenuStage MainMenuStage { get; private set; }
        [field: SerializeField, FoldoutGroup("Runtime")] public StandardStage StandardStage { get; private set; }
        private void Awake() {
            Log.ZLogTrace($"[{nameof(MainMenuContext)}][{nameof(Awake)}]");
        }
        private void OnEnable() {
            Log.ZLogTrace($"[{nameof(MainMenuContext)}][{nameof(OnEnable)}]");
            SL.Register(this);
        }
        private void Start() {
            Log.ZLogTrace($"[{nameof(MainMenuContext)}][{nameof(Start)}]");
        }
        private void OnDisable() {
            Log.ZLogTrace($"[{nameof(MainMenuContext)}][{nameof(OnDisable)}]");
            SL.Unregister(this);
        }
        private void OnDestroy() {
            Log.ZLogTrace($"[{nameof(MainMenuContext)}][{nameof(OnDestroy)}]");
        }
    }
    
    public partial class MainMenuContext : ISceneEntryPoint {
        public async UniTask OnInitialize(ISceneDataReader reader, IProgress<IProgressDataStore> prog, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(MainMenuContext)}][{nameof(OnInitialize)}]");
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            SL.TryGet(out AssetManager assetMgr);
            reader.TryRead(out MainMenuStage mainMenuStage);
            MainMenuStage = mainMenuStage;
            
            StandardStage = await assetMgr.AcquireAsync<StandardStage>(MainMenuStage.StandardStageRef, cts.Token);
            var beatmapScroll = await assetMgr.AcquireAsync<BeatmapScroll>(MainMenuStage.BeatmapScrollRef, cts.Token);
            var beatmapCell = await assetMgr.AcquireAsync<BeatmapCell>(MainMenuStage.BeatmapCellRef, cts.Token);
            
            using (var scope = new GameObjectX.NewGameObjectScope<MainMenuViewController>(false)) {
                scope.Component.Initialize(StandardStage, beatmapScroll, beatmapCell);
            }
            
            // TODO: Remove fake loading
            var store = new ProgressDataStore<LoadingProgressData>();
            for (int i = 0; i < 9; i++) {
                float percent = i / 10f;
                prog.Report(store.SetData(new LoadingProgressData(ProgressCycle.CurrentInitialize, percent, "MainMenu.OnInitialize")));
                await UniTask.WaitForSeconds(0.04f, cancellationToken: cts.Token);
            }
        }
        public UniTask OnEnter(ISceneDataReader reader, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(MainMenuContext)}][{nameof(OnEnter)}]");
            return UniTask.CompletedTask;
        }
        public UniTask OnExit(ISceneDataWriter writer, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(MainMenuContext)}][{nameof(OnExit)}]");
            return UniTask.CompletedTask;
        }
        public async UniTask OnFinalize(ISceneDataWriter writer, IProgress<IProgressDataStore> prog, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(MainMenuContext)}][{nameof(OnFinalize)}]");
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            SL.TryGet(out AssetManager assetMgr);
            
            assetMgr.Release(MainMenuStage.BeatmapCellRef);
            assetMgr.Release(MainMenuStage.BeatmapScrollRef);
            assetMgr.Release(MainMenuStage.StandardStageRef);
            
            await MainMenuStage.OnBeforeSceneUnloadedAsyncOperationAsync(prog, cts.Token);
        }
#if UNITY_EDITOR
        public UniTask OnEditorFirstPreInitialize(ISceneDataWriter writer, CancellationToken ct) => UniTask.CompletedTask;
#endif
    }
}