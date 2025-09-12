using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace VTBeat {
    // TODO: Fix hard-coded localization keys
    [DefaultExecutionOrder(-40)]
    public class BootstrapContext : MonoBehaviour {
        [field: Title("Project")]
        [field: SerializeField] public AssetReferenceScene CoreScene { get; private set; }
        [field: Title("Scene")]
        [field: SerializeField] public TMP_Text OperationDesc { get; private set; }
        
        private async UniTaskVoid Start() {
            // Only initialize no update (turn off in settings)
            OperationDesc.text = await GetLocalizedStringAsync("UI_Bootstrap", "OperationDesc_InitializeAsync");
            await Addressables.InitializeAsync().ToUniTask();
            // Check and update catalog
            OperationDesc.text = await GetLocalizedStringAsync("UI_Bootstrap", "OperationDesc_UpdateCatalogs");
            List<string> catalog = await Addressables.CheckForCatalogUpdates(); // Return an empty list if nothing to update
            if (catalog.Count > 0) {
                await Addressables.UpdateCatalogs(catalog);
            }
            // Bootstrap job is just loading into the latest Core scene (who will handle the rest)
            OperationDesc.text = await GetLocalizedStringAsync("UI_Bootstrap", "OperationDesc_LoadSceneAsync");
            var hdl = CoreScene.LoadSceneAsync(LoadSceneMode.Additive);
            await hdl.ToUniTask();
            
            if (hdl.Status == AsyncOperationStatus.Succeeded) {
                // Unload this scene (Bootstrap)
                await SceneManager.UnloadSceneAsync(0).ToUniTask();
            }
            else {
                OperationDesc.text = GetLocalizedStringAsync("UI_Bootstrap", "OperationDesc_Error").GetAwaiter().GetResult();
            }
        }
        
        private async UniTask<string> GetLocalizedStringAsync(string tableName, string key) {
            var hdl = LocalizationSettings.StringDatabase.GetTableAsync(tableName);
            await hdl.ToUniTask();
            // Check for a result
            if (hdl.Status == AsyncOperationStatus.Failed) {
                return string.Empty;
            }
            var stringTable = hdl.Result;
            // Check for entry and return
            if (!stringTable.TryGetEntry(key, out StringTableEntry entry)) {
                return string.Empty;
            }
            return entry.GetLocalizedString();
        }
    }
}