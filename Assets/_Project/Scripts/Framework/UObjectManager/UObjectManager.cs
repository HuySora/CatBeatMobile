using System;
using UnityEngine;

namespace VTBeat.UnityObject {
    // TODO: Pooling, T : UObject
    public class UObjectManager {
        #region Create
        public readonly struct InstantiateScope<T> : IDisposable where T : MonoBehaviour {
            public readonly T Result;
            private readonly bool m_WasInitiallyActive;
            
            public InstantiateScope(T prefab) {
                m_WasInitiallyActive = prefab.gameObject.activeSelf;
                prefab.gameObject.SetActive(false);
                Result = UObject.Instantiate(prefab);
                prefab.gameObject.SetActive(m_WasInitiallyActive);
            }
            public void Dispose() {
                if (m_WasInitiallyActive) {
                    Result.gameObject.SetActive(true);
                }
            }
        }
        
        public InstantiateScope<T> CreateScope<T>(T prefab) where T : MonoBehaviour => new(prefab);
        
        public T Create<T>(T prefab) where T : UObject => UObject.Instantiate(prefab);
        public T Create<T>(T prefab, Transform parent) where T : UObject => UObject.Instantiate(prefab, parent);
        public T Create<T>(T prefab, Transform parent, bool worldPositionStays) where T : UObject => UObject.Instantiate(prefab, parent, worldPositionStays);
        public T Create<T>(T prefab, Vector3 position, Quaternion rotation) where T : UObject => UObject.Instantiate(prefab, position, rotation);
        public T Create<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : UObject => UObject.Instantiate(prefab, position, rotation, parent);
        #endregion
    }
}