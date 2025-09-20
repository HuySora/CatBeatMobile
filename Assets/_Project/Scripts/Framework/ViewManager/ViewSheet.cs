using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VTBeat.View {
#if UNITY_EDITOR
    public partial class ViewSheet : IPrepare {
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
    
    public partial class ViewSheet : MonoBehaviour {
        [field: Title("Scene")]
        [field: SerializeField] public RectTransform ParentTransform { get; private set; }
        
        // private readonly NavigationSheetCore m_Core = new();
        // public IPage ActivePage => m_Core.ActivePage;
        // public IReadOnlyCollection<IPage> Pages => m_Core.Pages;
        //
        // private void OnEnable() {
        //     m_Core.OnPageAttached += HandlePageAttached;
        // }
        // private void OnDisable() {
        //     m_Core.OnPageDetached -= HandlePageAttached;
        // }
        //
        // private void HandlePageAttached(IPage page) {
        //     if (page is not Component cpn) return;
        //     if (ParentTransform == null) return;
        //     
        //     cpn.transform.SetParent(ParentTransform, false);
        // }
        //
        // public async UniTask<IPage> AddAsync(IPage page, CancellationToken cancellationToken = default) {
        //     await m_Core.AddAsync(page, CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken).Token);
        //     return page;
        // }
        // public async UniTask<IPage> HideAsync(NavigationContext context, CancellationToken cancellationToken = default) {
        //     var targetPage = m_Core.ActivePage;
        //     await m_Core.HideAsync(context, CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken).Token);
        //     return targetPage;
        // }
        //
        // public async UniTask RemoveAllAsync(CancellationToken cancellationToken = default) {
        //     await m_Core.RemoveAllAsync(CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken).Token);
        // }
        // public async UniTask<IPage> RemoveAsync(IPage page, CancellationToken cancellationToken = default) {
        //     await m_Core.RemoveAsync(page, CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken).Token);
        //     return page;
        // }
        //
        // public async UniTask<IPage> ShowAsync(int index, NavigationContext context, CancellationToken cancellationToken = default) {
        //     await m_Core.ShowAsync(index, context, CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken).Token);
        //     return m_Core.ActivePage;
        // }
    }
}