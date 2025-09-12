using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VTBeat {
    [DefaultExecutionOrder(-40)]
    public class MainMenuContext : MonoBehaviour {
        [field: Title("Project")]
        [field: SerializeField] public MainMenuLevel AssetContext { get; private set; }
        [field: SerializeField] public LevelAsset GameplayLevel { get; private set; }
        
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