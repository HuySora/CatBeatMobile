using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using VTBeat.Collection;

namespace VTBeat.View {
#if UNITY_EDITOR
    public partial class ViewStack : IPrepare {
        [FoldoutGroup("Runtime", false)]
        private bool m_EditorDummyBool;
        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton), PropertyOrder(-1)]
        public bool Prepare() {
            bool isDirty = false;
            if (ParentTransform == null) {
                ParentTransform = GetComponent<RectTransform>();
                if (ParentTransform != null) isDirty = true;
            }
            return isDirty;
        }
    }
#endif
    
    public partial class ViewStack : MonoBehaviour {
        [field: SerializeField, FoldoutGroup("Scene")] public RectTransform ParentTransform { get; private set; }
        private UniqueStack<IView> m_UniqueViewStack;
        
        private void Awake() {
            m_UniqueViewStack = new UniqueStack<IView>();
        }
        
        public async UniTask<IView> PushAsync(IView view, ViewTransitionContext ctx, CancellationToken ct = default) {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, ct);
            
            m_UniqueViewStack.TryPeek(out IView prevView);
            
            if (m_UniqueViewStack.Contains(view)) {
                m_UniqueViewStack.Push(view);
                // TODO: Fix hard cast
                (view as MonoBehaviour)?.transform.SetAsLastSibling();
            }
            else {
                m_UniqueViewStack.Push(view);
                (view as MonoBehaviour)?.transform.SetParent(ParentTransform, false);
            }
            
            try {
                if (prevView != null) await prevView.OnExit(ctx, cts.Token);
                await view.OnEnter(ctx, cts.Token);
            }
            catch {
                m_UniqueViewStack.Remove(view);
                throw;
            }
            
            return view;
        }
        public async UniTask<IView> RemoveAsync(IView view, ViewTransitionContext ctx, CancellationToken ct = default) {
            if (!m_UniqueViewStack.Contains(view)) return view;
            
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, ct);
            
            int index = m_UniqueViewStack.IndexOf(view);
            m_UniqueViewStack.Remove(view);
            try {
                await view.OnExit(ctx, cts.Token);
            }
            catch {
                m_UniqueViewStack.Insert(index, view);
                throw;
            }
            
            return view;
        }
        public async UniTask<IView> PopAsync(ViewTransitionContext ctx, CancellationToken ct = default) {
            if (!m_UniqueViewStack.TryPeek(out IView prevView)) return null;
            
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, ct);
            
            m_UniqueViewStack.Pop();
            m_UniqueViewStack.TryPeek(out IView activeView);
            
            try {
                await prevView.OnExit(ctx, cts.Token);
                if (activeView != null) await activeView.OnEnter(ctx, cts.Token);
            }
            catch {
                if (prevView != null) m_UniqueViewStack.Add(prevView);
            }
            
            return activeView;
        }
    }
}