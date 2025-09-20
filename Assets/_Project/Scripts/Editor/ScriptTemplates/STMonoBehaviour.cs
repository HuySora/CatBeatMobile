using UnityEngine;

namespace ST__RootNamespace {
#if UNITY_EDITOR
    using MyBox;
    using Sirenix.OdinInspector;
    public partial class ST__ScriptName : IPrepare {
        [FoldoutGroup("Runtime", false)]
        private bool m_EditorDummyBool;
        
        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton), PropertyOrder(-1)]
        public virtual bool Prepare() {
            bool isDirty = false;
            
            return isDirty;
        }
    }
#endif
    
    public partial class ST__ScriptName : MonoBehaviour {
    }
}