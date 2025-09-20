using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MackySoft.Navigathena;
using MackySoft.Navigathena.SceneManagement;
using MackySoft.Navigathena.SceneManagement.AddressableAssets;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using VTBeat.View;

namespace VTBeat.Stage.Core {
#if UNITY_EDITOR
    using UnityEditor;
    
    [CreateAssetMenu(menuName = $"VTBeat/Stage/S_{nameof(EmptyStage)}", fileName = $"L_{nameof(EmptyStage)}")]
    public partial class EmptyStage : IPrepare {
        public override bool Prepare() {
            bool isDirty = false;
            return isDirty | base.Prepare();
        }
    }
#endif
    
    public partial class EmptyStage : StageAsset, ISceneData {
        public override ISceneIdentifier GetSceneIdentifier() => new AddressableSceneIdentifier("Stages/Empty/Empty.unity");
        public override ISceneData GetSceneData() => this;
        public override async UniTask OnBeforeSceneUnloadedAsyncOperationAsync(IProgress<IProgressDataStore> prog, CancellationToken ct) {
            // TODO: Remove fake loading
            var store = new ProgressDataStore<LoadingProgressData>();
            for (int i = 0; i < 9; i++) {
                float percent = i / 10f;
                prog.Report(store.SetData(new LoadingProgressData(ProgressCycle.PreviousBeforeSceneUnloaded, percent, "EmptyStage.BeforeSceneUnload")));
                await UniTask.WaitForSeconds(0.02f, cancellationToken: ct);
            }
        }
    }
}