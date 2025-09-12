using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VTBeat {
    public abstract class LevelAsset : ScriptableObject {
        [field: SerializeField] public AssetReferenceScene SceneAssetReference { get; private set; }
        public virtual UniTask OnBeforeSceneLoadAsync() => UniTask.CompletedTask;
        public virtual UniTask OnAfterSceneUnloadAsync() => UniTask.CompletedTask;
    }
}