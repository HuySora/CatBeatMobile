#if UNITY_EDITOR
using UnityEngine;

namespace VTBeat.Extensions {
    public static class TransformX {
        public static bool TryFindFirstComponent<T>(this Transform transform, out T result, string gameObjName = "") where T : Component {
            result = null;
            bool checkName = !string.IsNullOrEmpty(gameObjName);
            
            if (checkName) {
                T[] cpns = transform.GetComponentsInChildren<T>(true);
                foreach (var c in cpns) {
                    if (c.gameObject.name != gameObjName) continue;
                    
                    result = c;
                    return true;
                }
            }
            else {
                result = transform.GetComponentInChildren<T>(true);
                if (result != null) {
                    return true;
                }
            }
            
            return false;
        }
    }
}
#endif