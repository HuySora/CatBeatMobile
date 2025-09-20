using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using VTBeat.Extensions;
using VTBeat.Stage.Standard;
using VTBeat.UnityObject;
using VTBeat.View;
using static VTBeat.LoggerMainMenuStage;

namespace VTBeat.Stage.MainMenu {
#if UNITY_EDITOR
    using UnityEditor;
    
    public partial class MainMenuViewController : IPrepare {
        [FoldoutGroup("Runtime", false)]
        private bool m_EditorDummyBool;
        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton), PropertyOrder(-1)]
        public bool Prepare() {
            bool isDirty = false;
            return isDirty;
        }
    }
#endif
    
    public partial class MainMenuViewController : MonoBehaviour {
        [field: SerializeField, FoldoutGroup("Project")] public StandardStage StandardStage { get; private set; }
        [field: SerializeField, FoldoutGroup("Project")] public BeatmapScroll BeatmapScrollPrefab { get; private set; }
        [field: SerializeField, FoldoutGroup("Project")] public BeatmapCell BeatmapCellPrefab { get; private set; }
        [field: SerializeField, FoldoutGroup("Runtime")] public BeatmapScroll BeatmapScroll { get; private set; }
        [field: SerializeField, FoldoutGroup("Runtime")] public BeatmapCell BeatmapCell { get; private set; }
        private CancellationTokenSource m_MainSheetBeatmapScrollTokenSource;
        
        public void Initialize(StandardStage standardStage, BeatmapScroll beatmapScroll, BeatmapCell beatmapCell) {
            Log.ZLogTrace($"[{nameof(MainMenuViewController)}][{nameof(Initialize)}]");
            StandardStage = standardStage;
            BeatmapScrollPrefab = beatmapScroll;
            BeatmapCellPrefab = beatmapCell;
        }
        private void Awake() {
            Log.ZLogTrace($"[{nameof(MainMenuViewController)}][{nameof(Awake)}]");
            m_MainSheetBeatmapScrollTokenSource = new CancellationTokenSource();
        }
        private void OnEnable() {
            Log.ZLogTrace($"[{nameof(MainMenuViewController)}][{nameof(OnEnable)}]");
            SL.Register(this);
            SL.TryGet(out UObjectManager uObjMgr);
            SL.TryGet(out ViewManager viewMgr);
            
            // Safe direct usage because we know this is a prefab for BeatmapScroll
            BeatmapCell = BeatmapCellPrefab;
            using (var scope = uObjMgr.CreateScope(BeatmapScrollPrefab)) {
                BeatmapScroll = scope.Result;
                BeatmapScroll.Initialize(BeatmapCell);
            }
            
            // TODO: Remove mock
            var mockData = new List<BeatmapData>();
            for (int i = 0; i < 50; i++) {
                mockData.Add(new BeatmapData {
                    BeatmapTitle = $"{i} Phút Hơn",
                    AuthorsLabel = "Pháo x Masew",
                    BackgroundImage = SpriteX.CreateRandomColorSprite(864, 192)
                });
            }
            BeatmapScroll.UpdateContents(mockData);
            BeatmapScroll.ScrollTo(0, 0.3f, FancyEase.InOutQuint);
            
            viewMgr.PushMainAsync(BeatmapScroll, null, CancellationTokenSourceX.CancelAndCreateLinkedToken(ref m_MainSheetBeatmapScrollTokenSource, destroyCancellationToken))
                .ForgetEx($"[{nameof(MainMenuViewController)}][{nameof(ViewManager.PushMainAsync)}.{nameof(BeatmapScroll)}] ");
        }
        private void Start() {
            Log.ZLogTrace($"[{nameof(MainMenuViewController)}][{nameof(Start)}]");
        }
        private void OnDisable() {
            Log.ZLogTrace($"[{nameof(MainMenuViewController)}][{nameof(OnDisable)}]");
            SL.TryGet(out ViewManager viewMgr);
            
            viewMgr.RemoveMainAsync(BeatmapScroll, null, CancellationTokenSourceX.CancelAndCreateLinkedToken(ref m_MainSheetBeatmapScrollTokenSource, destroyCancellationToken))
                .ContinueWith(page => {
                    if (page is MonoBehaviour mono) {
                        Destroy(mono.gameObject);
                    }
                }).ForgetEx($"[{nameof(MainMenuViewController)}][{nameof(ViewManager.PopMainAsync)}.{nameof(BeatmapScroll)}] ");
            
            SL.Unregister(this);
        }
        private void OnDestroy() {
            Log.ZLogTrace($"[{nameof(MainMenuViewController)}][{nameof(OnDestroy)}]");
        }
    }
}