using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using FancyScrollView;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using VTBeat.Stage;
using static VTBeat.LoggerViewManager;

namespace VTBeat.View {
#if UNITY_EDITOR
    using UnityEditor;
    
    public partial class BeatmapScroll : IPrepare {
        [FoldoutGroup("Runtime", false)]
        private bool m_EditorDummyBool;
        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton), PropertyOrder(-1)]
        public bool Prepare() {
            bool isDirty = false;
            return isDirty;
        }
    }
#endif
    
    public partial class BeatmapScroll : FancyScrollRect<BeatmapData, BeatmapScrollContext> {
        protected override float CellSize => m_CellSize;
        [SerializeField] private float m_CellSize;
        protected override GameObject CellPrefab => m_CellPrefab;
        [SerializeField] private GameObject m_CellPrefab;
        
        [field: SerializeField, FoldoutGroup("Config")] public FancyEase EaseType { get; private set; }
        
        public void Initialize(BeatmapCell cellPrefab) {
            Log.ZLogTrace($"[{nameof(BeatmapScroll)}][{nameof(Initialize)}]");
            m_CellPrefab = cellPrefab.gameObject;
            m_CellSize = m_CellPrefab.GetComponent<RectTransform>().sizeDelta.y;
            Context.OnCellClicked += HandleCellClicked;
            Context.OnPlayButtonClicked += HandlePlayButtonClicked;
            base.Initialize();
        }
        
        private void HandleCellClicked(BeatmapCell cell) {
            Context.SelectedIndex = cell.Index;
            ScrollTo(cell.Index, 0.3f, EaseType);
        }
        private void HandlePlayButtonClicked(BeatmapCell cell) {
            SL.TryGet(out StageManager stageMgr);
            // TODO: BeatmapCell should contain which stage to load
            SL.TryGet(out MainMenuContext sceneCtx);
        }
        
        public new void Refresh() => base.Refresh();
        public new void UpdateContents(IList<BeatmapData> dataList) => base.UpdateContents(dataList);
        public new void ScrollTo(int index, float duration, FancyEase easing, float alignment = 0.5f, Action onComplete = null) => base.ScrollTo(index, duration, easing, alignment, onComplete);
    }
    
    public partial class BeatmapScroll : IView {
        public UniTask OnExit(ViewTransitionContext context, CancellationToken cancellationToken = default) => UniTask.CompletedTask;
        public UniTask OnEnter(ViewTransitionContext context, CancellationToken cancellationToken = default) {
            Refresh();
            return UniTask.CompletedTask;
        }
    }
}