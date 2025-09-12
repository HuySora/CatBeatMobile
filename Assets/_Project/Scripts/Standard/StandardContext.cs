using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VTBeat {
    [DefaultExecutionOrder(-40)]
    public class StandardContext : MonoBehaviour {
        [field: Title("Project")]
        [field: SerializeField] public StandardLevel AssetContext { get; private set; }
        [field: SerializeField] public LevelAsset MainMenuLevel { get; private set; }
        
        public event Action OnSceneInitialized;
        
        private void Awake() {
            SL.Register(this);
        }
        private void Start() {
            OnSceneInitialized?.Invoke();
        }
        private void OnDestroy() {
            SL.Unregister(this);
        }
    }
}