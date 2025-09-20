using System.Runtime.CompilerServices;
using Cysharp.Text;

#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace VTBeat.Extensions {
    public static class StringX {
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
            return ZString.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", r, g, b, str);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string RTColorByHash(this string str, string color) {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(color)) return str;
            
            // Get a deterministic hash
            int hash = color.GetHashCode();
            
            // Convert hash to RGB (take 24 bits)
            byte r = (byte)((hash >> 16) & 0xFF);
            byte g = (byte)((hash >> 8) & 0xFF);
            byte b = (byte)(hash & 0xFF);
            
            // Format as #RRGGBB
            return ZString.Format("<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", r, g, b, str);
        }
        
#if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToAddressablesEditorKey(this string key) {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            var entry = settings.FindAssetEntry(key);
            return entry != null ? entry.address.RTColorByHash(key).RTColorByHash(key) : key.RTColorByHash();
        }
#endif
    }
}