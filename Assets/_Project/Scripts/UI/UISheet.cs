using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NavStack;
using NavStack.Internal;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VTBeat {
    public partial class UISheet : MonoBehaviour {
        [field: Title("Scene")]
        [field: SerializeField] public RectTransform ParentTransform { get; private set; }
        
        private readonly NavigationSheetCore m_Core = new();
        
        private void OnEnable() {
            OnPageAttached += HandlePageAttached;
        }
        private void OnDisable() {
            OnPageAttached -= HandlePageAttached;
        }
        
        private void HandlePageAttached(IPage page) {
            if (page is not Component cpn) return;
            if (ParentTransform == null) return;
            
            cpn.transform.SetParent(ParentTransform);
        }
    }
    
    public partial class UISheet : INavigationSheet {
        public IPage ActivePage => m_Core.ActivePage;
        public IReadOnlyCollection<IPage> Pages => m_Core.Pages;
        
        public event Action<IPage> OnPageAttached {
            add => m_Core.OnPageAttached += value;
            remove => m_Core.OnPageAttached -= value;
        }
        public event Action<IPage> OnPageDetached {
            add => m_Core.OnPageDetached += value;
            remove => m_Core.OnPageDetached -= value;
        }
        
        public event Action<(IPage Previous, IPage Current)> OnNavigating {
            add => m_Core.OnNavigating += value;
            remove => m_Core.OnNavigating -= value;
        }
        public event Action<(IPage Previous, IPage Current)> OnNavigated {
            add => m_Core.OnNavigated += value;
            remove => m_Core.OnNavigated -= value;
        }
        
        public async UniTask AddAsync(IPage page, CancellationToken cancellationToken = default) {
            await m_Core.AddAsync(page, CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken).Token);
        }
        public async UniTask HideAsync(NavigationContext context, CancellationToken cancellationToken = default) {
            await m_Core.HideAsync(context, CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken).Token);
        }
        
        public async UniTask RemoveAllAsync(CancellationToken cancellationToken = default) {
            await m_Core.RemoveAllAsync(CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken).Token);
        }
        public async UniTask RemoveAsync(IPage page, CancellationToken cancellationToken = default) {
            await m_Core.RemoveAsync(page, CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken).Token);
        }
        
        public async UniTask ShowAsync(int index, NavigationContext context, CancellationToken cancellationToken = default) {
            await m_Core.ShowAsync(index, context, CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken).Token);
        }
    }
}