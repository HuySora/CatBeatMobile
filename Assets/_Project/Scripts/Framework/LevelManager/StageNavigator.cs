using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using MackySoft.Navigathena;
using MackySoft.Navigathena.SceneManagement;
using MackySoft.Navigathena.SceneManagement.Unsafe;
using MackySoft.Navigathena.SceneManagement.Utilities;
using MackySoft.Navigathena.Transitions;
using UnityEngine.SceneManagement;
using VTBeat.Extensions;
using static VTBeat.LoggerStageManager;

namespace VTBeat.Stage {
    public partial class StageNavigator : ISceneNavigator {
        private enum TransitionStage {
            EditorFirstPreInitializing = 0,
            Initializing = 1,
            Entering = 2,
            Entered = 3
        }
        
        private readonly struct SceneState {
            public ISceneIdentifier Identifier { get; }
            public ISceneHandle Handle { get; }
            public ISceneEntryPoint EntryPoint { get; }
            public Scene Scene { get; }
            public TransitionStage TransitionStage { get; }
            
            public SceneState(ISceneIdentifier identifier, ISceneHandle handle, ISceneEntryPoint entryPoint, Scene scene, TransitionStage transitionStage = TransitionStage.Initializing) {
                Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
                Handle = handle ?? throw new ArgumentNullException(nameof(handle));
                EntryPoint = entryPoint ?? throw new ArgumentNullException(nameof(entryPoint));
                Scene = scene;
                TransitionStage = transitionStage;
            }
        }
        
        /// <summary>
        ///     History of all scenes, including the current scene.
        ///     Since interrupt processing may occur on ISceneEntryPoint events, the history must be updated prior to calling the
        ///     event.
        /// </summary>
        public IReadOnlyCollection<IReadOnlySceneHistoryEntry> History => m_History;
        private readonly History<SceneHistoryEntry> m_History = new();
        private readonly Dictionary<ISceneIdentifier, SceneState> m_LoadedSceneId2State = new();
        
        /// <summary>
        ///     A counter to prevent <see cref="OnSceneLoaded(Scene, LoadSceneMode)" /> from causing a double initialization
        ///     process.
        /// </summary>
        private readonly ProcessCounter m_ProcessCounter = new();
        
        private readonly ITransitionDirector m_DefaultTransitionDirector;
        private readonly ISceneProgressFactory m_SceneProgressFactory;
        
        private SceneState? m_CurrentSceneState;
        private TransitionStage? m_CurrentTransitionEnterStage;
        private TransitionStage? m_PreviousTransitionEnterStage;
        private TransitionDirectorState? m_RunningTransitionDirectorState;
        private CancellationTokenSource m_CurrentCancellationTokenSource;
        
        private bool m_HasInitialized;
        
        public StageNavigator() : this(null, null) { }
        public StageNavigator(ITransitionDirector defaultTransitionDirector, ISceneProgressFactory sceneProgressFactory) {
            m_DefaultTransitionDirector = defaultTransitionDirector ?? TransitionDirector.Empty();
            m_SceneProgressFactory = sceneProgressFactory ?? new StandardSceneProgressFactory();
        }
        
        public async UniTask Initialize() {
            ThrowIfDisposed();
            
            if (m_HasInitialized) return;
            m_HasInitialized = true;
            
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // NOTE: Even multi-scene editing in the editor must be performed within Start to ensure that the scene is fully loaded.
            var (scene, entryPoint) = SceneNavigatorHelper.FindFirstEntryPointInAllScenes();
            if (entryPoint == null) return;
            
            await InitializeFirstEntryPoint(scene, entryPoint, CancellationToken.None);
        }
        /// <summary>
        ///     When a new scene is loaded while this SceneNavigator is not initialized, this SceneNavigator is initialized if an
        ///     <see cref="ISceneEntryPoint" /> exists in the scene.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            // Ignore when the scene is loaded by the SceneNavigator itself.
            if (m_ProcessCounter.IsProcessing) return;
            if (m_CurrentSceneState != null) return;
            if (!scene.TryGetComponentInScene(out ISceneEntryPoint entryPoint, true)) return;
            
            InitializeFirstEntryPoint(scene, entryPoint, CancellationToken.None)
                .ForgetEx($"[{nameof(StageNavigator)}][{nameof(OnSceneLoaded)}]");
        }
        private async UniTask InitializeFirstEntryPoint(Scene scene, ISceneEntryPoint entryPoint, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            
            m_CurrentTransitionEnterStage = TransitionStage.Initializing;
            ISceneIdentifier identifier = new BuiltInSceneIdentifier(scene.name);
            m_CurrentSceneState = new SceneState(identifier, identifier.CreateHandle(), entryPoint, scene);
            m_LoadedSceneId2State[identifier] = m_CurrentSceneState.Value;
            SceneDataStore dataStore = new();
            m_History.Push(new SceneHistoryEntry(identifier, m_DefaultTransitionDirector, dataStore));
            
            using var _ = m_ProcessCounter.Increment();
            CancellationToken ct = CancelCurrentAndCreateLinkedToken(cancellationToken);
            
            Log.ZLogDebug($"[{nameof(StageNavigator)}][{nameof(InitializeFirstEntryPoint)}] Starting: {identifier}");
            
            try {
#if UNITY_EDITOR
                await entryPoint.OnEditorFirstPreInitialize(dataStore.Writer, ct);
#endif
                ct.ThrowIfCancellationRequested();
                // PREF: Check what player loop timing that go immediately after MonoBehaviour.Start()
                await UniTask.Yield(PlayerLoopTiming.Update);
                m_CurrentTransitionEnterStage = TransitionStage.Initializing;
                await entryPoint.OnInitialize(dataStore.Reader, Progress.Create<IProgressDataStore>(null), ct);
                
                ct.ThrowIfCancellationRequested();
                m_CurrentTransitionEnterStage = TransitionStage.Entering;
                await entryPoint.OnEnter(dataStore.Reader, ct);
                m_CurrentTransitionEnterStage = TransitionStage.Entered;
            }
            catch (OperationCanceledException) {
                Log.ZLogWarning($"[{nameof(StageNavigator)}][{nameof(InitializeFirstEntryPoint)}] Canceled: {identifier}");
                throw;
            }
        }
        
        public async UniTask Push(LoadSceneRequest request, CancellationToken cancellationToken = default) {
            Log.ZLogDebug($"[{nameof(StageNavigator)}][{nameof(Push)}] Request: {request.Scene}");
            ThrowIfDisposed();
            ThrowIfNotInitialized();
            cancellationToken.ThrowIfCancellationRequested();
            
            using var _ = m_ProcessCounter.Increment();
            CancellationToken ct = CancelCurrentAndCreateLinkedToken(cancellationToken);
            
            Log.ZLogDebug($"[{nameof(StageNavigator)}][{nameof(Push)}] Starting: {request.Scene}");
            
            try {
                // TODO: Refactor this currentSceneEntry actually are previousSceneEntry in this context
                if (m_History.TryPeek(out SceneHistoryEntry currentSceneEntry)) {
                    await ExitCurrentScene(currentSceneEntry, ct);
                }
                
                await EnsureStartTransitionDirectorAsync(request.TransitionDirector, ct);
                
                IProgressDataStore progressDataStore = m_SceneProgressFactory.CreateProgressDataStore();
                // await TryFinalizeAndUnloadCurrentScene(currentSceneEntry, progressDataStore, m_RunningTransitionDirectorState.Value.Progress, ct);
                await SceneNavigatorHelper.TryExecuteInterruptOperation(request.InterruptOperation, m_RunningTransitionDirectorState.Value.Progress, ct);
                
                m_CurrentSceneState = await GetSceneStateAsync(request.Scene, m_SceneProgressFactory, progressDataStore, m_RunningTransitionDirectorState.Value.Progress, ct);
                m_CurrentTransitionEnterStage = m_CurrentSceneState.Value.TransitionStage;
                
                SceneDataStore sceneDataStore = new(request.Data);
                m_History.Push(new SceneHistoryEntry(request.Scene, request.TransitionDirector ?? m_DefaultTransitionDirector, sceneDataStore));
                
                await EnterSceneSequenceAsync(sceneDataStore.Reader, ct);
            }
            catch (OperationCanceledException) {
                Log.ZLogWarning($"[{nameof(StageNavigator)}][{nameof(Push)}] Canceled: {request.Scene}");
                throw;
            }
        }
        public async UniTask Pop(PopSceneRequest request, CancellationToken cancellationToken = default) {
            Log.ZLogDebug($"[{nameof(StageNavigator)}][{nameof(Pop)}] Request");
            ThrowIfDisposed();
            ThrowIfNotInitialized();
            cancellationToken.ThrowIfCancellationRequested();
            if (m_History.Count <= 1) {
                Log.ZLogWarning($"[{nameof(StageNavigator)}][{nameof(Pop)}] Canceled: Nothing to pop");
                return;
            }
            
            using var _ = m_ProcessCounter.Increment();
            CancellationToken ct = CancelCurrentAndCreateLinkedToken(cancellationToken);
            
            SceneHistoryEntry currentSceneEntry = m_History.Pop();
            SceneHistoryEntry previousScene = m_History.Peek();
            
            Log.ZLogDebug($"[{nameof(StageNavigator)}][{nameof(Pop)}] Starting: {currentSceneEntry.Scene}");
            
            try {
                await ExitCurrentScene(currentSceneEntry, ct);
                
                await EnsureStartTransitionDirectorAsync(request.OverrideTransitionDirector ?? currentSceneEntry.TransitionDirector, ct);
                
                IProgressDataStore progressDataStore = m_SceneProgressFactory.CreateProgressDataStore();
                await FinalizeAndUnloadCurrentSceneAsync(currentSceneEntry, progressDataStore, m_RunningTransitionDirectorState.Value.Progress, ct);
                await SceneNavigatorHelper.TryExecuteInterruptOperation(request.InterruptOperation, m_RunningTransitionDirectorState.Value.Progress, ct);
                
                m_CurrentSceneState = await GetSceneStateAsync(previousScene.Scene, m_SceneProgressFactory, progressDataStore, m_RunningTransitionDirectorState.Value.Progress, ct);
                m_CurrentTransitionEnterStage = m_CurrentSceneState.Value.TransitionStage;
                
                await EnterSceneSequenceAsync(previousScene.DataStore.Reader, ct);
            }
            catch (OperationCanceledException) {
                Log.ZLogWarning($"[{nameof(StageNavigator)}][{nameof(Pop)}] Canceled: {currentSceneEntry.Scene}");
                throw;
            }
        }
        
        public UniTask Change(LoadSceneRequest request, CancellationToken cancellationToken = default) {
            Log.ZLogError($"[{nameof(StageNavigator)}][{nameof(Change)}] NotImplemented: {request.Scene}");
            return UniTask.CompletedTask;
        }
        
        public async UniTask Replace(LoadSceneRequest request, CancellationToken cancellationToken = default) {
            Log.ZLogDebug($"[{nameof(StageNavigator)}][{nameof(Replace)}] Request: {request.Scene}");
            ThrowIfDisposed();
            ThrowIfNotInitialized();
            cancellationToken.ThrowIfCancellationRequested();
            
            if (m_History.Count == 0) {
                Log.ZLogWarning($"[{nameof(StageNavigator)}][{nameof(Replace)}] Canceled: Nothing to replace");
                return;
            }
            
            using var _ = m_ProcessCounter.Increment();
            CancellationToken ct = CancelCurrentAndCreateLinkedToken(cancellationToken);
            
            Log.ZLogDebug($"[{nameof(StageNavigator)}][{nameof(Replace)}] Starting: {request.Scene}");
            
            try {
                SceneHistoryEntry currentSceneEntry = m_History.Peek();
                
                await ExitCurrentScene(currentSceneEntry, ct);
                
                await EnsureStartTransitionDirectorAsync(request.TransitionDirector, ct);
                
                IProgressDataStore progressDataStore = m_SceneProgressFactory.CreateProgressDataStore();
                await FinalizeAndUnloadCurrentSceneAsync(currentSceneEntry, progressDataStore, m_RunningTransitionDirectorState.Value.Progress, ct);
                await SceneNavigatorHelper.TryExecuteInterruptOperation(request.InterruptOperation, m_RunningTransitionDirectorState.Value.Progress, ct);
                
                m_CurrentSceneState = await GetSceneStateAsync(request.Scene, m_SceneProgressFactory, progressDataStore, m_RunningTransitionDirectorState.Value.Progress, ct);
                m_CurrentTransitionEnterStage = m_CurrentSceneState.Value.TransitionStage;
                
                SceneDataStore sceneDataStore = new(request.Data);
                m_History.Pop();
                m_History.Push(new SceneHistoryEntry(request.Scene, request.TransitionDirector ?? m_DefaultTransitionDirector, sceneDataStore));
                
                await EnterSceneSequenceAsync(sceneDataStore.Reader, ct);
            }
            catch (OperationCanceledException) {
                Log.ZLogWarning($"[{nameof(StageNavigator)}][{nameof(Replace)}] Canceled: {request.Scene}");
                throw;
            }
        }
        public async UniTask Reload(ReloadSceneRequest request, CancellationToken cancellationToken = default) {
            Log.ZLogDebug($"[{nameof(StageNavigator)}][{nameof(Reload)}] Request");
            ThrowIfDisposed();
            ThrowIfNotInitialized();
            cancellationToken.ThrowIfCancellationRequested();
            
            if (m_History.Count == 0) {
                Log.ZLogWarning($"[{nameof(StageNavigator)}][{nameof(Reload)}] Canceled: Nothing to reload");
                return;
            }
            
            using var _ = m_ProcessCounter.Increment();
            CancellationToken ct = CancelCurrentAndCreateLinkedToken(cancellationToken);
            
            SceneHistoryEntry currentSceneEntry = m_History.Peek();
            
            Log.ZLogDebug($"[{nameof(StageNavigator)}][{nameof(Reload)}] Starting: {currentSceneEntry.Scene}");
            
            try {
                await ExitCurrentScene(currentSceneEntry, ct);
                
                await EnsureStartTransitionDirectorAsync(request.OverrideTransitionDirector ?? currentSceneEntry.TransitionDirector, ct);
                
                IProgressDataStore progressDataStore = m_SceneProgressFactory.CreateProgressDataStore();
                await FinalizeAndUnloadCurrentSceneAsync(currentSceneEntry, progressDataStore, m_RunningTransitionDirectorState.Value.Progress, ct);
                await SceneNavigatorHelper.TryExecuteInterruptOperation(request.InterruptOperation, m_RunningTransitionDirectorState.Value.Progress, ct);
                
                m_CurrentSceneState = await GetSceneStateAsync(currentSceneEntry.Scene, m_SceneProgressFactory, progressDataStore, m_RunningTransitionDirectorState.Value.Progress, ct);
                m_CurrentTransitionEnterStage = m_CurrentSceneState.Value.TransitionStage;
                
                await EnterSceneSequenceAsync(currentSceneEntry.DataStore.Reader, ct);
            }
            catch (OperationCanceledException) {
                Log.ZLogWarning($"[{nameof(StageNavigator)}][{nameof(Reload)}] Canceled: {currentSceneEntry.Scene}");
                throw;
            }
        }
        
        private async UniTask ExitCurrentScene(SceneHistoryEntry currentSceneEntry, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            if (currentSceneEntry == null) return;
            if (m_CurrentSceneState == null) return;
            if (m_PreviousTransitionEnterStage is not >= TransitionStage.Entering) return;
            
            await m_CurrentSceneState.Value.EntryPoint.OnExit(currentSceneEntry.DataStore.Writer, cancellationToken);
        }
        
        private async UniTask EnsureStartTransitionDirectorAsync(ITransitionDirector transitionDirector, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            if (m_RunningTransitionDirectorState != null) return;
            
            m_RunningTransitionDirectorState = SceneNavigatorHelper.CreateTransitionHandle(transitionDirector ?? m_DefaultTransitionDirector);
            await m_RunningTransitionDirectorState.Value.Handle.Start(cancellationToken);
        }
        
        private async UniTask FinalizeAndUnloadCurrentSceneAsync(SceneHistoryEntry currentSceneEntry, IProgressDataStore progressDataStore, IProgress<IProgressDataStore> progress, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            if (currentSceneEntry == null) return;
            if (m_CurrentSceneState == null) return;
            
            await m_CurrentSceneState.Value.EntryPoint.OnFinalize(currentSceneEntry.DataStore.Writer, progress, cancellationToken);
            
            // NOTE: If the current scene is the only scene, create an empty scene to prevent an exception from being thrown when unloading.
            if (SceneManager.sceneCount < 2) {
                cancellationToken.ThrowIfCancellationRequested();
                await NavigathenaBlankSceneIdentifier.Instance.CreateHandle().Load(cancellationToken: cancellationToken);
            }
            
            cancellationToken.ThrowIfCancellationRequested();
            await m_CurrentSceneState.Value.Handle.Unload(m_SceneProgressFactory.CreateProgress(progressDataStore, progress), cancellationToken);
            m_CurrentSceneState = null;
        }
        
        private async UniTask<SceneState> GetSceneStateAsync(ISceneIdentifier sceneIdentifier, ISceneProgressFactory sceneProgressFactory, IProgressDataStore progressDataStore, IProgress<IProgressDataStore> progress, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            if (sceneIdentifier == null) {
                throw new ArgumentNullException(nameof(sceneIdentifier));
            }
            
            // TODO: Better checking with SceneManager.GetSceneByName()
            if (!m_LoadedSceneId2State.TryGetValue(sceneIdentifier, out SceneState value)) {
                ISceneHandle hdl = sceneIdentifier.CreateHandle();
                Scene scene = await hdl.Load(sceneProgressFactory.CreateProgress(progressDataStore, progress), cancellationToken);
                
                cancellationToken.ThrowIfCancellationRequested();
                await NavigathenaBlankSceneIdentifier.Instance.CreateHandle().Unload(cancellationToken: cancellationToken);
                
                var entry = scene.GetComponentInScene<ISceneEntryPoint>(true);
                
                return m_LoadedSceneId2State[sceneIdentifier] = new SceneState(
                    sceneIdentifier,
                    hdl,
                    entry,
                    scene
                );
            }
            
            return value;
        }
        
        private async UniTask EnterSceneSequenceAsync(ISceneDataReader reader, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            
            SceneManager.SetActiveScene(m_CurrentSceneState.Value.Scene);
            // PREF: Check what player loop timing that go immediately after MonoBehaviour.Start()
            await UniTask.Yield(PlayerLoopTiming.Update);
            if (m_CurrentTransitionEnterStage is TransitionStage.Initializing) {
                await m_CurrentSceneState.Value.EntryPoint.OnInitialize(reader, m_RunningTransitionDirectorState.Value.Progress, cancellationToken);
            }
            
            cancellationToken.ThrowIfCancellationRequested();
            await m_RunningTransitionDirectorState.Value.Handle.End(cancellationToken);
            m_RunningTransitionDirectorState = null;
            
            cancellationToken.ThrowIfCancellationRequested();
            m_CurrentTransitionEnterStage = TransitionStage.Entering;
            await m_CurrentSceneState.Value.EntryPoint.OnEnter(reader, cancellationToken);
            m_CurrentTransitionEnterStage = TransitionStage.Entered;
        }
        
        /// <summary>
        ///     Cancel the current CancellationTokenSource and create a new linked token source.
        /// </summary>
        private CancellationToken CancelCurrentAndCreateLinkedToken(CancellationToken cancellationToken) {
            m_PreviousTransitionEnterStage = m_CurrentTransitionEnterStage;
            m_CurrentTransitionEnterStage = null;
            
            m_CurrentCancellationTokenSource?.Cancel();
            m_CurrentCancellationTokenSource?.Dispose();
            m_CurrentCancellationTokenSource = new CancellationTokenSource();
            return CancellationTokenSource.CreateLinkedTokenSource(m_CurrentCancellationTokenSource.Token, cancellationToken).Token;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfNotInitialized() {
            if (!m_HasInitialized) {
                throw new SceneNavigationException($"{nameof(StageNavigator)} has not been initialized.");
            }
        }
    }
    
    public partial class StageNavigator : IUnsafeSceneNavigator {
        private class StandardSceneHistoryBuilder : SceneHistoryBuilderBase {
            private readonly StageNavigator m_Owner;
            private readonly int m_Version;
            
            public StandardSceneHistoryBuilder(StageNavigator owner) : base(owner.m_History) {
                m_Owner = owner;
                m_Version = owner.m_ProcessCounter.Version;
            }
            
            public override void Build() {
                if (m_Owner.m_ProcessCounter.IsProcessing) {
                    throw new InvalidOperationException($"Process is currently ongoing in the {nameof(StageNavigator)}.");
                }
                if (m_Owner.m_ProcessCounter.Version != m_Version) {
                    throw new InvalidOperationException($"The {nameof(StageNavigator)} has been updated since the history builder was created.");
                }
                if (m_Owner.m_CurrentSceneState == null) {
                    throw new InvalidOperationException("The current scene state is null.");
                }
                
                m_Owner.m_History.Clear();
                foreach (SceneHistoryEntry entry in Enumerable.Reverse(m_History)) {
                    m_Owner.m_History.Push(entry);
                }
                Log.ZLogDebug($"[{nameof(StageNavigator)}] Scene history has been changed directly");
            }
        }
        
        ISceneHistoryBuilder IUnsafeSceneNavigator.GetHistoryBuilderUnsafe() {
            ThrowIfDisposed();
            ThrowIfNotInitialized();
            
            if (m_ProcessCounter.IsProcessing) {
                throw new InvalidOperationException($"Process is currently ongoing in the {nameof(StageNavigator)}.");
            }
            return new StandardSceneHistoryBuilder(this);
        }
    }
    
    public partial class StageNavigator : IDisposable {
        private bool m_IsDisposed;
        public void Dispose() {
            if (m_IsDisposed) {
                return;
            }
            m_IsDisposed = true;
            
            m_CurrentCancellationTokenSource?.Cancel();
            m_CurrentCancellationTokenSource?.Dispose();
            m_CurrentCancellationTokenSource = null;
            
            m_CurrentSceneState = null;
            m_History.Clear();
            
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfDisposed() {
            if (m_IsDisposed) {
                throw new ObjectDisposedException(nameof(StageNavigator));
            }
        }
    }
}