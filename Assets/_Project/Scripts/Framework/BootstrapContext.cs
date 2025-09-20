using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MyBox;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using VTBeat.Asset;
using VTBeat.Extensions;

namespace VTBeat {
#if UNITY_EDITOR
    using UnityEditor;
    
    public partial class BootstrapContext : IPrepare {
        [FoldoutGroup("Runtime", false)]
        private bool m_EditorDummyBool;
        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton), PropertyOrder(-1)]
        public bool Prepare() {
            bool isDirty = false;
            if (CoreSceneRef.editorAsset == null) {
                string path = "Assets/_Project/_Stages/Core/Core.unity";
                var coreSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (coreSceneAsset != null) {
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    CoreSceneRef = new AssetReferenceScene(guid);
                    isDirty = true;
                }
            }
            return isDirty;
        }
    }
#endif
    
    [DefaultExecutionOrder(-40)]
    public partial class BootstrapContext : MonoBehaviour {
        [field: SerializeField, FoldoutGroup("Project")] public AssetReferenceScene CoreSceneRef { get; private set; }
        [field: SerializeField, FoldoutGroup("Scene")] public TMP_Text OperationDesc { get; private set; }
        
        // TODO: Fix hard-coded localization keys
        private async UniTaskVoid Start() {
            // Only initialize no update (turn off in settings)
            OperationDesc.text = await GetLocalizedStringAsync("UI_Bootstrap", "OperationDesc_InitializeAsync");
            await Addressables.InitializeAsync().ToUniTask();
            
            // Return an empty list if nothing to update
            OperationDesc.text = await GetLocalizedStringAsync("UI_Bootstrap", "OperationDesc_UpdateCatalogs");
            List<string> catalog = await Addressables.CheckForCatalogUpdates();
            if (catalog.Count > 0) {
                await Addressables.UpdateCatalogs(catalog);
            }
            
            // Bootstrap job is just loading into the latest Core scene (who will handle the rest)
            OperationDesc.text = await GetLocalizedStringAsync("UI_Bootstrap", "OperationDesc_LoadSceneAsync");
            // TODO: Check what happen when Core scene doesn't exist
            try {
                await CoreSceneRef.LoadSceneAsync(LoadSceneMode.Additive);
                await SceneManager.UnloadSceneAsync(gameObject.scene);
            }
            catch (Exception e) {
                Console.WriteLine(e);
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