using System;
using UnityEngine;

namespace VTBeat.Extensions {
    public static class GameObjectX {
        public readonly struct NewGameObjectScope<T> : IDisposable where T : Component {
            public readonly GameObject GameObject;
            public readonly T Component;
            
            private readonly bool m_WasInactive;
            
            public NewGameObjectScope(bool isActive = false) : this(null, isActive) { }
            public NewGameObjectScope(Transform parent, bool isActive = false) {
                GameObject = new GameObject(typeof(T).Name);
                
                GameObject.SetActive(isActive);
                m_WasInactive = !isActive;
                
                GameObject.transform.SetParent(parent, false);
                Component = GameObject.AddComponent<T>();
            }
            
            public void Dispose() {
                if (m_WasInactive) {
                    GameObject.SetActive(true);
                }
            }
        }
    }
}