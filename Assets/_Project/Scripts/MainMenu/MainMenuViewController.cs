using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VTBeat {
    public class MainMenuViewController : MonoBehaviour {
        [field: Title("Runtime")]
        [field: SerializeField] public SceneSwitchButton SwitchButton { get; private set; }
        
        private void Awake() {
            SL.TryGet(out MainMenuContext ctx);
            
            ctx.OnSceneInitialized += () => gameObject.SetActive(true);
        }
        private void OnEnable() {
            SL.TryGet(out MainMenuContext ctx);
            SL.TryGet(out UObjectManager uObjMgr);
            SL.TryGet(out ViewManager viewMgr);
            
            // TODO: This will always create so we don't need to check
            uObjMgr.TryCreate(ctx.AssetContext.SceneSwitchButton, out SceneSwitchButton switchBtn);
            SwitchButton = switchBtn;
            
            // Start as inactive and add to sheet
            SwitchButton.gameObject.SetActive(false);
            viewMgr.MainSheet.AddAsync(SwitchButton);
            // Initializing
            SwitchButton.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            SwitchButton.Initialize();
            SwitchButton.Button.onClick.AddListener(() => {
                SL.TryGet(out LevelManager levelMgr);
                levelMgr.LoadSceneAsync(ctx.GameplayLevel).ContinueWith(scene => {
                    SL.TryGet(out LevelManager levelMgr);
                    levelMgr.SetActiveScene(scene);
                });
            });
            // Show
            viewMgr.MainSheet.ShowAsync(0, new() {
                Parameters = new() {
                    { "label", "Standard" }
                }
            });
        }
        private void OnDisable() {
            SL.TryGet(out ViewManager viewMgr);
            
            viewMgr.MainSheet.RemoveAsync(SwitchButton);
        }
    }
}