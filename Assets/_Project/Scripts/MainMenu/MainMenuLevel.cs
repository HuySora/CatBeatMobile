using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VTBeat {
    [CreateAssetMenu(menuName = "VTBeat/Level/Level_MainMenu")]
    public class MainMenuLevel : LevelAsset {
        [field: SerializeField] public AssetReferenceSceneSwitchButton SceneSwitchButton { get; private set; }
        public override async UniTask OnBeforeSceneLoadAsync() {
        }
    }
}