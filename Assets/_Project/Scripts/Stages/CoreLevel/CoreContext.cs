using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MackySoft.Navigathena;
using MackySoft.Navigathena.SceneManagement;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using VTBeat.Asset;
using VTBeat.Event;
using VTBeat.Extensions;
using VTBeat.Sound;
using VTBeat.UnityObject;
using VTBeat.View;
using static VTBeat.LoggerCoreStage;

namespace VTBeat.Stage.Core {
#if UNITY_EDITOR
    using UnityEditor;
    
    public partial class CoreContext : IPrepare {
        [FoldoutGroup("Runtime", false)]
        private bool m_EditorDummyBool;
        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton), PropertyOrder(-1)]
        public bool Prepare() {
            bool isDirty = false;
            if (CoreStageRef.editorAsset == null) {
                isDirty |= AssetDatabaseX.TryFindFirstAsset<AssetReferenceUObject<CoreStage>, CoreStage>(out CoreStageRef);
            }
            if (MainViewStack == null) {
                isDirty |= gameObject.scene.TryFindFirstBehaviour(out ViewStack mainView, "MainCanvas");
                MainViewStack = mainView;
            }
            if (OverlayViewSheet == null) {
                isDirty |= gameObject.scene.TryFindFirstBehaviour(out ViewSheet overlayView, "OverlayCanvas");
                OverlayViewSheet = overlayView;
            }
            if (NotifyViewStack == null) {
                isDirty |= gameObject.scene.TryFindFirstBehaviour(out ViewStack notifyView, "PopupCanvas");
                NotifyViewStack = notifyView;
            }
            if (LoadingCanvas == null) {
                isDirty |= gameObject.scene.TryFindFirstBehaviour(out Canvas loadingCanvas, "LoadingCanvas");
                LoadingCanvas = loadingCanvas;
            }
            return isDirty;
        }
    }
#endif
    
    [DefaultExecutionOrder(-50)]
    public partial class CoreContext : MonoBehaviour {
        [field: SerializeField, FoldoutGroup("Project")] public AssetReferenceUObject<CoreStage> CoreStageRef;
        [field: SerializeField, FoldoutGroup("Scene")] public ViewStack MainViewStack { get; private set; }
        [field: SerializeField, FoldoutGroup("Scene")] public ViewSheet OverlayViewSheet { get; private set; }
        [field: SerializeField, FoldoutGroup("Scene")] public ViewStack NotifyViewStack { get; private set; }
        [field: SerializeField, FoldoutGroup("Scene")] public Canvas LoadingCanvas { get; private set; }
        [field: SerializeField, FoldoutGroup("Runtime")] public CoreStage CoreStage { get; private set; }
        [field: SerializeField, FoldoutGroup("Runtime")] public EmptyStage EmptyStage { get; private set; }
        [field: SerializeField, FoldoutGroup("Runtime")] public StageAsset EntryStage { get; private set; }
    }
    
    public partial class CoreContext : ISceneEntryPoint {
        public async UniTask OnInitialize(ISceneDataReader reader, IProgress<IProgressDataStore> prog, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(CoreContext)}][{nameof(OnInitialize)}]");
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            // TODO: Somehow separate service registering with initializing
            if (!SRDebug.IsInitialized) SRDebug.Init();
            
            if (!SL.TryGet(out EventManager _)) SL.Register(new EventManager());
            if (!SL.TryGet(out AssetManager assetMgr)) SL.Register(assetMgr = new AssetManager());
            
            CoreStage = await assetMgr.AcquireAsync<CoreStage>(CoreStageRef, cts.Token);
            EmptyStage = await assetMgr.AcquireAsync<EmptyStage>(CoreStage.EmptyStageRef, cts.Token);
            EntryStage = await assetMgr.AcquireAsync<StageAsset>(CoreStage.EntryStageRef, cts.Token);
            
            if (!SL.TryGet(out UObjectManager uObjMgr)) SL.Register(uObjMgr = new UObjectManager());
            if (!SL.TryGet(out SoundManager _)) SL.Register(new SoundManager());
            
            var loadingViewPrefab = await assetMgr.AcquireAsync<LoadingView>(CoreStage.LoadingViewRef, cts.Token);
            LoadingView loadingView;
            using (var scope = uObjMgr.CreateScope(loadingViewPrefab)) {
                loadingView = scope.Result;
                loadingView.Initialize(LoadingCanvas);
            }
            
            if (!SL.TryGet(out ViewManager _)) SL.Register(new ViewManager(MainViewStack, OverlayViewSheet, NotifyViewStack, loadingView));
            if (!SL.TryGet(out StageManager _)) SL.Register(new StageManager());
        }
        public UniTask OnEnter(ISceneDataReader reader, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(CoreContext)}][{nameof(OnEnter)}]");
            
            // MUST: Do not put ct into this, it will break Navigathena
            HandleSceneEntryPointOnEnter().ForgetEx($"[{nameof(CoreContext)}][{nameof(HandleSceneEntryPointOnEnter)}]");
            return UniTask.CompletedTask;
        }
        private async UniTask HandleSceneEntryPointOnEnter(CancellationToken ct = default) {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            SL.TryGet(out StageManager stageMgr);
            SL.TryGet(out ViewManager viewMgr);
            
            // Make sure StageNavigator finished it cycle for CoreScene loading (since we immediately load into EntryScene with this)
            await UniTask.WaitForSeconds(1f, cancellationToken: cts.Token);
            await stageMgr.Initialize(EmptyStage, cts.Token);
            
            Scene bootstrapScene = SceneManager.GetSceneByBuildIndex(0);
            if (bootstrapScene.IsValid()) {
                await SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(0));
                await stageMgr.WaitUntilUnloadAsync(bootstrapScene.name, cts.Token);
            }
            
            await stageMgr.LoadAsync(EntryStage, viewMgr.GetLoadingTransition(), cts.Token);
        }
        public UniTask OnExit(ISceneDataWriter writer, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(CoreContext)}][{nameof(OnExit)}]");
            return UniTask.CompletedTask;
        }
        public UniTask OnFinalize(ISceneDataWriter writer, IProgress<IProgressDataStore> prog, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(CoreContext)}][{nameof(OnFinalize)}]");
            SL.TryGet(out AssetManager assetMgr);
            
            assetMgr.Release(CoreStage.EntryStageRef);
            assetMgr.Release(CoreStage.EmptyStageRef);
            // TODO: Unloading this might cause bug
            assetMgr.Release(CoreStage.LoadingViewRef);
            assetMgr.Release(CoreStageRef);
            
            return UniTask.CompletedTask;
        }
#if UNITY_EDITOR
        public UniTask OnEditorFirstPreInitialize(ISceneDataWriter writer, CancellationToken ct) => UniTask.CompletedTask;
#endif
    }
}