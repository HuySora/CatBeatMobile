using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MackySoft.Navigathena;
using MackySoft.Navigathena.SceneManagement;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using VTBeat.Asset;

namespace VTBeat.Stage {
#if UNITY_EDITOR
    using UnityEditor;
    
    public partial class StageAsset : IPrepare {
        [FoldoutGroup("Runtime", false)]
        private bool m_EditorDummyBool;
        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton), PropertyOrder(-1)]
        public virtual bool Prepare() {
            bool isDirty = false;
            if (AssetRef.editorAsset != this) {
                AssetRef = new AssetReferenceUObject<StageAsset>();
                AssetRef.SetEditorAsset(this);
                isDirty = true;
            }
            
            return isDirty;
        }
    }
#endif
    
    public abstract partial class StageAsset : ScriptableObject {
        [field: FoldoutGroup("Project"), SerializeField] public AssetReferenceUObject<StageAsset> AssetRef;
        public abstract ISceneIdentifier GetSceneIdentifier();
        public virtual ISceneData GetSceneData() => new SceneDataEmpty();
        public virtual IAsyncOperation GetBeforeSceneLoadAsyncOperation() => AsyncOperation.Empty();
        public virtual UniTask OnBeforeSceneUnloadedAsyncOperationAsync(IProgress<IProgressDataStore> prog, CancellationToken ct) => UniTask.CompletedTask;
    }
}