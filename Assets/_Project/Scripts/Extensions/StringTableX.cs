using System.Runtime.CompilerServices;
using UnityEngine.Localization.Tables;

namespace VTBeat.Extensions {
    public static class StringTableX {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetEntry(this StringTable strTbl, string key, out StringTableEntry entry) {
            entry = strTbl.GetEntry(key);
            return entry != null;
        }
    }
}