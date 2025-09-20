using System.Collections.Generic;
using MyBox;
using MyBox.EditorTools;
using UnityEditor;
using UnityEngine;

namespace VTBeat._Project.Scripts.Editor.Tools {
    public class MyBoxIPrepareX {
        [MenuItem("Tools/MyBox/IPrepare/Scenes", priority = 1)]
        public static void PrepareScene() => IPrepareFeature.RunIPrepare();
        [MenuItem("Tools/MyBox/IPrepare/ProjectAssets", priority = 2)]
        public static void PrepareProjectAssets() {
            GUID[] guids = AssetDatabase.FindAssetGUIDs("t:ScriptableObject");
            var prepareList = new List<(ScriptableObject asset, IPrepare prepare)>();
            foreach (GUID guid in guids) {
                var asset = AssetDatabase.LoadAssetByGUID<ScriptableObject>(guid);
                if (asset is not IPrepare prepare) continue;
                
                prepareList.Add((asset, prepare));
            }
            
            foreach (var prepare in prepareList) {
                bool changed = prepare.prepare.Prepare();
                if (!changed) continue;
                
                EditorUtility.SetDirty(prepare.asset);
                Debug.Log(prepare.asset.name + "." + prepare.asset.GetType().Name + ": Changed on Prepare", prepare.asset);
            }
        }
        [MenuItem("Tools/MyBox/IPrepare/Both")]
        public static void PrepareBoth() {
            PrepareScene();
            PrepareProjectAssets();
        }
    }
}