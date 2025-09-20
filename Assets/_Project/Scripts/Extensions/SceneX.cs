#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VTBeat.Extensions {
    public static class SceneX {
        public static bool TryFindFirstBehaviour<T>(this Scene scene, out T result, string gameObjName = "") where T : Behaviour {
            result = null;
            bool checkName = !string.IsNullOrEmpty(gameObjName);
            
            var gObjs = scene.GetRootGameObjects();
            
            if (checkName) {
                foreach (var root in gObjs) {
                    // Check root
                    if (root.name == gameObjName) {
                        result = root.GetComponent<T>();
                        if (result != null) return true;
                    }
                    // Check child
                    Transform childTransform = root.transform.Find(gameObjName);
                    if (childTransform == null) continue;
                    
                    result = childTransform.GetComponent<T>();
                    if (result != null) return true;
                }
            }
            else {
                foreach (var root in gObjs) {
                    result = root.GetComponentInChildren<T>(true);
                    if (result != null) return true;
                }
            }
            
            return false;
        }
    }
}
#endif