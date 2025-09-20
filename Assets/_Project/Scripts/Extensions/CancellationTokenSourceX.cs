using System.Runtime.CompilerServices;
using System.Threading;

namespace VTBeat.Extensions {
    public static class CancellationTokenSourceX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CancellationToken CancelAndCreateLinkedToken(ref CancellationTokenSource toBeCanceledCts, CancellationToken targetCt) {
            toBeCanceledCts?.Cancel();
            toBeCanceledCts?.Dispose();
            toBeCanceledCts = new CancellationTokenSource();
            return CancellationTokenSource.CreateLinkedTokenSource(toBeCanceledCts.Token, targetCt).Token;
        }
    }
}