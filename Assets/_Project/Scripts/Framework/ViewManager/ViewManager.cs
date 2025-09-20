using System.Collections.Concurrent;
using System.Threading;
using Cysharp.Threading.Tasks;
using MackySoft.Navigathena.Transitions;

namespace VTBeat.View {
    public enum ViewLayer {
        MainView = 0,
        OverlayView = 1,
        PopupView = 2,
    }
    
    // TODO: Caller doesn't exactly know which view are stack-based or plainly showing requested view
    public class ViewManager {
        private readonly ViewStack m_MainView;
        private readonly ViewSheet m_OverlayView;
        private readonly ViewStack m_NotifyView;
        private readonly LoadingView m_LoadingView;
        
        public ViewManager(ViewStack mainView, ViewSheet overlayView, ViewStack notifyView, LoadingView loadingView) {
            m_MainView = mainView;
            m_OverlayView = overlayView;
            m_NotifyView = notifyView;
            m_LoadingView = loadingView;
        }
        
        #region LoadingView
        public ITransitionDirector GetLoadingTransition() => m_LoadingView.LoadingTransition;
        #endregion
        
        #region MainView
        public async UniTask<T> PushMainAsync<T>(T view, ViewTransitionContext ctx = null, CancellationToken ct = default) where T : IView {
            (bool isCanceled, _) = await m_MainView.PushAsync(view, ctx, ct).SuppressCancellationThrow();
            if (isCanceled) {
                return default;
            }
            
            return view;
        }
        public async UniTask<T> RemoveMainAsync<T>(T view, ViewTransitionContext ctx = null, CancellationToken ct = default) where T : IView {
            (bool isCanceled, _) = await m_MainView.RemoveAsync(view, ctx, ct).SuppressCancellationThrow();
            if (isCanceled) {
                return default;
            }
            
            return view;
        }
        public async UniTask<T> PopMainAsync<T>(ViewTransitionContext ctx = null, CancellationToken ct = default) where T : IView {
            (bool isCanceled, IView view) = await m_MainView.PopAsync(ctx, ct).SuppressCancellationThrow();
            if (isCanceled || view is not T typedView) {
                return default;
            }
            
            return typedView;
        }
        #endregion
        
        // #region OverlayView
        // private readonly ConcurrentDictionary<IView, int> m_View2OverlayIndex = new ConcurrentDictionary<IView, int>();
        // public async UniTask<T> ShowOverlayAsync<T>(T view, ViewTransitionContext ctx = null, CancellationToken ct = default) where T : IView {
        //     bool isCanceled;
        //     if (!m_View2OverlayIndex.TryGetValue(view, out int index)) {
        //         (isCanceled, _) = await m_OverlayView.AddAsync(view, ct).SuppressCancellationThrow();
        //         if (isCanceled) {
        //             return default;
        //         }
        //         index = m_OverlayView.Pages.Count;
        //     }
        //     
        //     (isCanceled, _) = await m_OverlayView.ShowAsync(index, ctx ?? m_DefaultNavigationContext, ct).SuppressCancellationThrow();
        //     if (isCanceled) {
        //         return default;
        //     }
        //     
        //     return view;
        // }
        // public async UniTask<T> RemoveOverlayAsync<T>(T view, ViewTransitionContext ctx = null, CancellationToken ct = default) where T : IView {
        //     if (!m_View2OverlayIndex.TryGetValue(view, out int index)) {
        //         return default;
        //     }
        //     bool isCanceled;
        //     if (view.Equals(m_OverlayView.ActivePage)) {
        //         (isCanceled, _) = await m_OverlayView.HideAsync(ctx, ct).SuppressCancellationThrow();
        //         if (isCanceled) {
        //             return default;
        //         }
        //     }
        //     
        //     (isCanceled, _) = await m_OverlayView.RemoveAsync(view, ct).SuppressCancellationThrow();
        //     if (isCanceled) {
        //         return default;
        //     }
        //     
        //     return view;
        // }
        // #endregion
        //
        // #region NotifyView
        // public async UniTask<T> PushNotifyAsync<T>(T view, ViewTransitionContext ctx = null, CancellationToken ct = default) where T : IView {
        //     (bool isCanceled, _) = await m_NotifyView.PushAsync(view, ctx ?? m_DefaultNavigationContext, ct).SuppressCancellationThrow();
        //     if (isCanceled) {
        //         return default;
        //     }
        //     
        //     return view;
        // }
        // public async UniTask<T> PopNotifyAsync<T>(ViewTransitionContext ctx = null, CancellationToken ct = default) where T : IView {
        //     (bool isCanceled, IView view) = await m_NotifyView.PopAsync(ctx ?? m_DefaultNavigationContext, ct).SuppressCancellationThrow();
        //     if (isCanceled || view is not T typedView) {
        //         return default;
        //     }
        //     
        //     return typedView;
        // }
        // #endregion
    }
}