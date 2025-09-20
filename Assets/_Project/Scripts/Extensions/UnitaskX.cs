using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using static VTBeat.LoggerGlobal;

namespace VTBeat.Extensions {
    public static class UnitaskX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForgetEx(this UniTask task, string ctx = "") {
            task.Forget(ex => Log.ZLogError($"{ctx}Exception: {ex}"));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForgetEx<T>(this UniTask<T> task, string ctx) {
            task.Forget(ex => Log.ZLogError($"{ctx}Exception: {ex}"));
        }
    }
}