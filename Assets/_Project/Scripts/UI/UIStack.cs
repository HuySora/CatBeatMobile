using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NavStack;
using NavStack.Internal;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VTBeat {
    public partial class UIStack : MonoBehaviour {
        [field: Title("Scene")]
        [field: SerializeField] public RectTransform ParentTransform { get; private set; }
        
        private readonly NavigationStackCore m_Core = new();
        
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
    
    public partial class UIStack : INavigationStack {
        public event Action<IPage> OnPageAttached {
            add => m_Core.OnPageAttached += value;
            remove => m_Core.OnPageAttached -= value;
        }
        public event Action<IPage> OnPageDetached {
            add => m_Core.OnPageDetached += value;
            remove => m_Core.OnPageDetached -= value;
        }
        
        public event Action<(IPage Previous, IPage Current)> OnNavigated {
            add => m_Core.OnNavigated += value;
            remove => m_Core.OnNavigated -= value;
        }
        public event Action<(IPage Previous, IPage Current)> OnNavigating {
            add => m_Core.OnNavigating += value;
            remove => m_Core.OnNavigating -= value;
        }
        
        public IPage ActivePage => m_Core.ActivePage;
        public IReadOnlyCollection<IPage> Pages => m_Core.Pages;
        
        public async UniTask PopAsync(NavigationContext context, CancellationToken cancellationToken = default) {
            await m_Core.PopAsync(context, CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken).Token);
        }
        
        public async UniTask PushAsync(IPage page, NavigationContext context, CancellationToken cancellationToken = default) {
            await m_Core.PushAsync(() => new(page), context, CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken).Token);
        }
        public async UniTask PushAsync(Func<UniTask<IPage>> factory, NavigationContext context, CancellationToken cancellationToken = default) {
            await m_Core.PushAsync(factory, context, CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken).Token);
        }
    }
}