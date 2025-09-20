using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MackySoft.Navigathena;
using MackySoft.Navigathena.SceneManagement;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using static VTBeat.LoggerStandardStage;

namespace VTBeat.Stage.Standard {
#if UNITY_EDITOR
    using UnityEditor;
    
    public partial class StandardContext : IPrepare {
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
    public partial class StandardContext : MonoBehaviour {
        [field: SerializeField, FoldoutGroup("Runtime")] private StandardStage m_StandardStage;
        private void Awake() {
            Log.ZLogTrace($"[{nameof(StandardContext)}][{nameof(Awake)}]");
        }
        private void Start() {
            Log.ZLogTrace($"[{nameof(StandardContext)}][{nameof(Start)}]");
        }
        private void OnEnable() {
            Log.ZLogTrace($"[{nameof(StandardContext)}][{nameof(OnEnable)}]");
            SL.Register(this);
        }
        private void OnDisable() {
            Log.ZLogTrace($"[{nameof(StandardContext)}][{nameof(OnDisable)}]");
            SL.Unregister(this);
        }
        private void OnDestroy() {
            Log.ZLogTrace($"[{nameof(StandardContext)}][{nameof(OnDestroy)}]");
        }
    }
    
    public partial class StandardContext : ISceneEntryPoint {
        public UniTask OnInitialize(ISceneDataReader reader, IProgress<IProgressDataStore> prog, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(StandardContext)}][{nameof(OnInitialize)}]");
            return UniTask.CompletedTask;
        }
        public UniTask OnEnter(ISceneDataReader reader, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(StandardContext)}][{nameof(OnEnter)}]");
            return UniTask.CompletedTask;
        }
        public UniTask OnExit(ISceneDataWriter writer, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(StandardContext)}][{nameof(OnExit)}]");
            return UniTask.CompletedTask;
        }
        public async UniTask OnFinalize(ISceneDataWriter writer, IProgress<IProgressDataStore> prog, CancellationToken ct) {
            Log.ZLogTrace($"[{nameof(StandardContext)}][{nameof(OnFinalize)}]");
            await m_StandardStage.OnBeforeSceneUnloadedAsyncOperationAsync(prog, ct);
        }
#if UNITY_EDITOR
        public UniTask OnEditorFirstPreInitialize(ISceneDataWriter writer, CancellationToken ct) => UniTask.CompletedTask;
#endif
    }
}