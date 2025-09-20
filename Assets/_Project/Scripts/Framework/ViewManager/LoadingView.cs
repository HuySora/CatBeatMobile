using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Freya;
using MackySoft.Navigathena;
using MackySoft.Navigathena.Transitions;
using MyBox;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VTBeat.Extensions;
using static VTBeat.LoggerViewManager;

namespace VTBeat.View {
#if UNITY_EDITOR
    using UnityEditor;
    
    public partial class LoadingView : IPrepare {
        [FoldoutGroup("Runtime", false)]
        private bool m_EditorDummyBool;
        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton), PropertyOrder(-1)]
        public bool Prepare() {
            bool isDirty = false;
            if (CanvasGroup == null) {
                isDirty |= transform.TryFindFirstComponent(out CanvasGroup out1);
                CanvasGroup = out1;
            }
            if (FillImage == null) {
                isDirty |= transform.TryFindFirstComponent(out Image out2, "Image_ProgressFill");
                FillImage = out2;
            }
            if (ProgressDesc == null) {
                isDirty |= transform.TryFindFirstComponent(out TMP_Text out3, "Desc_Message");
                ProgressDesc = out3;
            }
            return isDirty;
        }
    }
#endif
    
    public partial class LoadingView : MonoBehaviour {
        [field: SerializeField, FoldoutGroup("Scene")] public CanvasGroup CanvasGroup { get; private set; }
        [field: SerializeField, FoldoutGroup("Scene")] public Image FillImage { get; private set; }
        [field: SerializeField, FoldoutGroup("Scene")] public TMP_Text ProgressDesc { get; private set; }
        
        public Director LoadingTransition { get; private set; }
        
        public void Initialize(Canvas canvas) {
            CanvasGroup.alpha = 0;
            FillImage.fillAmount = 0;
            transform.SetParent(canvas.transform, false);
            LoadingTransition = new Director(this);
        }
        private void Awake() {
            Log.ZLogTrace($"[{nameof(LoadingView)}][{nameof(Awake)}]");
        }
        private void OnEnable() {
            Log.ZLogTrace($"[{nameof(LoadingView)}][{nameof(OnEnable)}]");
        }
        private void Start() {
            Log.ZLogTrace($"[{nameof(LoadingView)}][{nameof(Start)}]");
        }
        private void OnDisable() {
            Log.ZLogTrace($"[{nameof(LoadingView)}][{nameof(OnDisable)}]");
        }
        private void OnDestroy() {
            Log.ZLogTrace($"[{nameof(LoadingView)}][{nameof(OnDestroy)}]");
        }
        
        public class Director : ITransitionDirector {
            private readonly Handle m_HandleView;
            public Director(LoadingView view) {
                m_HandleView = new Handle(view);
            }
            public ITransitionHandle CreateHandle() => m_HandleView;
        }
        
        public partial class Handle : ITransitionHandle, IProgress<IProgressDataStore> {
            private readonly LoadingView m_View;
            public Handle(LoadingView view) {
                m_View = view;
            }
            public async UniTask Start(CancellationToken ct = default) {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, m_View.destroyCancellationToken);
                
                m_View.CanvasGroup.interactable = false;
                m_View.CanvasGroup.blocksRaycasts = false;
                try {
                    await m_View.CanvasGroup.DOFade(1f, 1f).SetEase(Ease.OutQuad).WithCancellation(cts.Token);
                }
                catch {
                    m_View.CanvasGroup.interactable = true;
                    m_View.CanvasGroup.blocksRaycasts = true;
                    throw;
                }
            }
            public async UniTask End(CancellationToken ct = default) {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, m_View.destroyCancellationToken);
                
                m_View.CanvasGroup.interactable = true;
                m_View.CanvasGroup.blocksRaycasts = true;
                try {
                    await m_View.CanvasGroup.DOFade(0f, 1f).SetEase(Ease.InQuad).WithCancellation(cts.Token);
                }
                finally {
                    m_View.CanvasGroup.interactable = false;
                    m_View.CanvasGroup.blocksRaycasts = false;
                }
            }
            
            public void Report(IProgressDataStore value) {
                if (!value.TryGetData(out LoadingProgressData data)) return;
                
                // TODO: Hardcoded value
                (float iMin, float iMax) = (0f, 1f);
                (float begin, float split1, float split2, float split3, float end) = (0f, 0.2f, 0.3f, 0.8f, 1f);
                float realProgress = data.Cycle switch {
                    ProgressCycle.PreviousFinalize => Mathfs.RemapClamped(iMin, iMax, begin, split1, data.Progress),
                    ProgressCycle.PreviousBeforeSceneUnloaded => Mathfs.RemapClamped(iMin, iMax, split1, split2, data.Progress),
                    ProgressCycle.CurrentBeforeSceneLoad => Mathfs.RemapClamped(iMin, iMax, split2, split3, data.Progress),
                    ProgressCycle.CurrentInitialize => Mathfs.RemapClamped(iMin, iMax, split3, end, data.Progress),
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                m_View.FillImage.DOFillAmount(Mathf.Clamp01(realProgress), 0.5f);
                m_View.ProgressDesc.text = data.Message;
            }
        }
    }
    
    // public partial class LoadingView : IView {
    //     public UniTask OnEnter(ViewTransitionContext context, CancellationToken cancellationToken = default) => UniTask.CompletedTask;
    //     public UniTask OnExit(ViewTransitionContext context, CancellationToken cancellationToken = default) => UniTask.CompletedTask;
    // }
}