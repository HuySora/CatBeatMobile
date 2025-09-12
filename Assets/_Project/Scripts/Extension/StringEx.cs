using System.Runtime.CompilerServices;
using UnityEngine.Localization.Tables;

namespace VTBeat {
    public static class StringEx {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RTColorByHash(this string str) {
            if (string.IsNullOrEmpty(str)) return str;
            
            // Get a deterministic hash
            int hash = str.GetHashCode();
            
            // Convert hash to RGB (take 24 bits)
            byte r = (byte)((hash >> 16) & 0xFF);
            byte g = (byte)((hash >> 8) & 0xFF);
            byte b = (byte)(hash & 0xFF);
            
            // Format as #RRGGBB
            return $"<color=#{r:X2}{g:X2}{b:X2}>{str}</color>";
        }
    }
}