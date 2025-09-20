using System.Threading;
using Cysharp.Threading.Tasks;

namespace VTBeat.View {
    public interface IView {
        UniTask OnEnter(ViewTransitionContext context, CancellationToken cancellationToken = default);
        UniTask OnExit(ViewTransitionContext context, CancellationToken cancellationToken = default);
    }
}