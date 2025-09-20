using MackySoft.Navigathena.SceneManagement;
using MackySoft.Navigathena.SceneManagement.AddressableAssets;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using VTBeat.Asset;
using VTBeat.Extensions;
using VTBeat.Stage.MainMenu;
using VTBeat.View;

namespace VTBeat.Stage.Core {
#if UNITY_EDITOR
    using UnityEditor;
    
    [CreateAssetMenu(menuName = $"VTBeat/Stage/S_{nameof(CoreStage)}", fileName = $"L_{nameof(CoreStage)}")]
    public partial class CoreStage : IPrepare {
        public override bool Prepare() {
            bool isDirty = false;
            if (EmptyStageRef.editorAsset == null) {
                isDirty |= AssetDatabaseX.TryFindFirstAsset<AssetReferenceUObject<EmptyStage>, EmptyStage>(out var emptyStageRef);
                EmptyStageRef = emptyStageRef;
            }
            if (LoadingViewRef.editorAsset == null) {
                isDirty |= AssetDatabaseX.TryFindFirstPrefab<AssetReferenceComponent<LoadingView>, LoadingView>(out var loadingViewRef);
                LoadingViewRef = loadingViewRef;
            }
            if (EntryStageRef.editorAsset == null) {
                isDirty |= AssetDatabaseX.TryFindFirstAsset<AssetReferenceUObject<MainMenuStage>, MainMenuStage>(out var mainStageRef);
                isDirty |= mainStageRef.TryConvert(out AssetReferenceUObject<StageAsset> entryStageRef);
                EntryStageRef = entryStageRef;
            }
            return isDirty | base.Prepare();
        }
    }
#endif
    
    public partial class CoreStage : StageAsset, ISceneData {
        [field: SerializeField, FoldoutGroup("Project")] public AssetReferenceUObject<EmptyStage> EmptyStageRef { get; private set; }
        [field: SerializeField, FoldoutGroup("Project")] public AssetReferenceComponent<LoadingView> LoadingViewRef { get; private set; }
        [field: SerializeField, FoldoutGroup("Project")] public AssetReferenceUObject<StageAsset> EntryStageRef { get; private set; }
        public override ISceneIdentifier GetSceneIdentifier() => new AddressableSceneIdentifier("Stages/Core/Core.unity");
        public override ISceneData GetSceneData() => this;
        // MUST: We can't preload core stage with this method since all game systems live inside it
        // public override IAsyncOperation GetBeforeSceneLoadAsyncOperation() => AsyncOperation.Empty();
        // public override UniTask OnBeforeSceneUnloadedAsyncOperationAsync(IProgress<IProgressDataStore> prog, CancellationToken ct) => UniTask.CompletedTask;
    }
}