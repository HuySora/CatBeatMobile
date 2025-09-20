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
using VTBeat.Stage.MainMenu;
using static VTBeat.LoggerStandardStage;

namespace VTBeat.Stage.Standard {
#if UNITY_EDITOR
    using UnityEditor;
    
    [CreateAssetMenu(menuName = $"VTBeat/Stage/S_{nameof(StandardStage)}", fileName = $"L_{nameof(StandardStage)}")]
    public partial class StandardStage : IPrepare {
        public override bool Prepare() {
            bool isDirty = false;
            if (MainMenuStageRef.editorAsset == null) {
                isDirty |= AssetDatabaseX.TryFindFirstAsset<AssetReferenceUObject<MainMenuStage>, MainMenuStage>(out var assetRef);
                MainMenuStageRef = assetRef;
            }
            return isDirty | base.Prepare();
        }
    }
#endif
    
    public partial class StandardStage : StageAsset, ISceneData {
        [field: SerializeField, FoldoutGroup("Project")] public AssetReferenceUObject<MainMenuStage> MainMenuStageRef { get; private set; }
        public override ISceneIdentifier GetSceneIdentifier() => new AddressableSceneIdentifier("Stages/Standard/Standard.unity");
        public override ISceneData GetSceneData() => this;
        public override IAsyncOperation GetBeforeSceneLoadAsyncOperation() => AsyncOperation.Create(ExecuteAsync);
        private async UniTask ExecuteAsync(IProgress<IProgressDataStore> prog, CancellationToken tk) {
            Log.ZLogTrace($"[{nameof(StandardStage)}][{nameof(GetBeforeSceneLoadAsyncOperation)}]");
            SL.TryGet(out AssetManager assetMgr);
            
            await assetMgr.AcquireAsync(new List<AssetReference> { MainMenuStageRef });
        }
        public override UniTask OnBeforeSceneUnloadedAsyncOperationAsync(IProgress<IProgressDataStore> prog, CancellationToken tk) {
            Log.ZLogTrace($"[{nameof(StandardStage)}][{nameof(OnBeforeSceneUnloadedAsyncOperationAsync)}]");
            SL.TryGet(out AssetManager assetMgr);
            
            assetMgr.Release(new List<AssetReference> { MainMenuStageRef });
            return UniTask.CompletedTask;
        }
    }
}