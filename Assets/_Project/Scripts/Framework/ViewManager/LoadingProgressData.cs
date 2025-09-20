namespace VTBeat.View {
    public enum ProgressCycle {
        PreviousFinalize = 0, // ISceneEntry.OnFinalize
        PreviousBeforeSceneUnloaded = 1, // StageAsset.OnBeforeSceneUnloadedAsyncOperationAsync()
        CurrentBeforeSceneLoad, // IAsyncOperation.ExecuteAsync (InterruptOp) -> StageAsset.GetBeforeSceneLoadAsyncOperation
        CurrentInitialize, // ISceneEntry.OnInitialize
    }
    
    public readonly struct LoadingProgressData {
        public ProgressCycle Cycle { get; }
        public float Progress { get; }
        public string Message { get; }
        
        public LoadingProgressData(ProgressCycle cycle, float prog, string msg) {
            Cycle = cycle;
            Progress = prog;
            Message = msg;
        }
    }
}