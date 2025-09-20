using System.IO;
using UnityEditor;

namespace ST__RootNamespace {
    public static class STMenu {
        private const string MonoBehaviourTemplateFilePath = "Assets/_Project/Scripts/ScriptTemplates/STMonoBehaviour.cs";
        
        [MenuItem("Assets/Create/VTBeat/MonoBehaviour", false, 80)]
        public static void CreateMonoBehaviourScript() {
            string fileName = "NewMonoBehaviourScript.cs";
            fileName = EditorUtility.SaveFilePanelInProject(
                "Create MonoBehaviour script",
                fileName,
                "cs",
                "Saved to",
                GetSelectedPath()
            );
            
            if (string.IsNullOrEmpty(fileName)) return;
            
            // Initialize tokens and load template file
            string rootNamespace = string.IsNullOrEmpty(EditorSettings.projectGenerationRootNamespace)
                ? "Assembly-CSharp"
                : EditorSettings.projectGenerationRootNamespace;
            string className = Path.GetFileNameWithoutExtension(fileName);
            string templateText = File.ReadAllText(MonoBehaviourTemplateFilePath);
            
            templateText = templateText.Replace("ST__ScriptName", className);
            templateText = templateText.Replace("ST__RootNamespace", rootNamespace);
            
            // Write to file
            File.WriteAllText(fileName, templateText);
            AssetDatabase.Refresh();
            
            // Focus the new file
            UObject newAsset = AssetDatabase.LoadAssetAtPath<UObject>(fileName);
            Selection.activeObject = newAsset;
            AssetDatabase.OpenAsset(newAsset);
        }
        
        private static string GetSelectedPath() {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (ProjectWindowUtil.IsFolder(Selection.activeInstanceID)) {
                return path;
            }
            return ProjectWindowUtil.GetContainingFolder(path);
        }
    }
}