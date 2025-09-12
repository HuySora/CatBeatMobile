using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SoraCore.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

namespace VTBeat {
    // NOTE: Make sure this loaded before any script using it GetService<T>
    [DefaultExecutionOrder(-50)]
    public class MainContext : SingletonBehaviour<MainContext> {
        [field: Title("Project")]
        [field: SerializeField] public LevelAsset EntryLevel { get; private set; }
        
        private void Awake() {
            SL.Register(new AssetManager());
            SL.Register(new UObjectManager());
            SL.Register(new SoundManager());
            SL.Register(new ViewManager());
            SL.Register(new LevelManager());
        }
        
        private async UniTaskVoid Start() {
            SRDebug.Init();
            SL.TryGet(out LevelManager levelMgr);
            
            await levelMgr.WaitUntilUnloadAsync(SceneManager.GetSceneByBuildIndex(0).name);
            // Dont destroy Core scene
            levelMgr.AddToPersistent(gameObject.scene);
            var entryScene = await levelMgr.LoadSceneAsync(EntryLevel);
            levelMgr.SetActiveScene(entryScene);
        }
    }
}