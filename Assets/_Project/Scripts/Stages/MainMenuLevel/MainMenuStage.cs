using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MackySoft.Navigathena;
using MackySoft.Navigathena.SceneManagement;
using MackySoft.Navigathena.SceneManagement.AddressableAssets;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VTBeat.Asset;
using VTBeat.Extensions;
using VTBeat.Stage.Standard;
using VTBeat.View;
using static VTBeat.LoggerMainMenuStage;

namespace VTBeat.Stage.MainMenu {
#if UNITY_EDITOR
    using UnityEditor;
    
    [CreateAssetMenu(menuName = $"VTBeat/Stage/S_{nameof(MainMenuStage)}", fileName = $"L_{nameof(MainMenuStage)}")]
    public partial class MainMenuStage : IPrepare {
        public override bool Prepare() {
            bool isDirty = false;
            if (StandardStageRef.editorAsset == null) {
                isDirty |= AssetDatabaseX.TryFindFirstAsset<AssetReferenceUObject<StandardStage>, StandardStage>(out var assetRef);
                StandardStageRef = assetRef;
            }
            if (BeatmapScrollRef.editorAsset == null) {
                isDirty |= AssetDatabaseX.TryFindFirstPrefab<AssetReferenceComponent<BeatmapScroll>, BeatmapScroll>(out var assetRef);
                BeatmapScrollRef = assetRef;
            }
            if (BeatmapCellRef.editorAsset == null) {
                isDirty |= AssetDatabaseX.TryFindFirstPrefab<AssetReferenceComponent<BeatmapCell>, BeatmapCell>(out var assetRef);
                BeatmapCellRef = assetRef;
            }
            return isDirty | base.Prepare();
        }
    }
#endif
    
    public partial class MainMenuStage : StageAsset, ISceneData {
        [field: SerializeField, FoldoutGroup("Project")] public AssetReferenceUObject<StandardStage> StandardStageRef { get; private set; }
        [field: SerializeField, FoldoutGroup("Project")] public AssetReferenceComponent<BeatmapScroll> BeatmapScrollRef { get; private set; }
        [field: SerializeField, FoldoutGroup("Project")] public AssetReferenceComponent<BeatmapCell> BeatmapCellRef { get; private set; }
        public override ISceneIdentifier GetSceneIdentifier() => new AddressableSceneIdentifier("Stages/MainMenu/MainMenu.unity");
        public override ISceneData GetSceneData() => this;
        public override IAsyncOperation GetBeforeSceneLoadAsyncOperation() => AsyncOperation.Create(ExecuteAsync);
        private async UniTask ExecuteAsync(IProgress<IProgressDataStore> prog, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(MainMenuStage)}][{nameof(GetBeforeSceneLoadAsyncOperation)}]");
            SL.TryGet(out AssetManager assetMgr);
            
            UniTask realTask = assetMgr.AcquireAsync(new List<AssetReference> { StandardStageRef, BeatmapScrollRef, BeatmapCellRef }, ct);
            
            // TODO: Remove fake loading
            var store = new ProgressDataStore<LoadingProgressData>();
            for (int i = 0; i < 9; i++) {
                float percent = i / 10f;
                prog.Report(store.SetData(new LoadingProgressData(ProgressCycle.CurrentBeforeSceneLoad, percent, "MainMenuStage.BeforeSceneLoad")));
                await UniTask.WaitForSeconds(0.1f, cancellationToken: ct);
            }
            
            await realTask;
        }
        public override UniTask OnBeforeSceneUnloadedAsyncOperationAsync(IProgress<IProgressDataStore> prog, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(MainMenuStage)}][{nameof(OnBeforeSceneUnloadedAsyncOperationAsync)}]");
            SL.TryGet(out AssetManager assetMgr);
            
            assetMgr.Release(new List<AssetReference> { StandardStageRef, BeatmapScrollRef, BeatmapCellRef });
            return UniTask.CompletedTask;
        }
    }
}