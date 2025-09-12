using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VTBeat {
    [CreateAssetMenu(menuName = "VTBeat/Level/Level_Standard")]
    public class StandardLevel : LevelAsset {
        [field: SerializeField] public AssetReferenceSceneSwitchButton SceneSwitchButton { get; private set; }
    }
}