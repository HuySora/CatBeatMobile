using FancyScrollView;
using MyBox;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VTBeat.Extensions;

namespace VTBeat.View {
#if UNITY_EDITOR
    using UnityEditor;
    
    public partial class BeatmapCell : IPrepare {
        [FoldoutGroup("Runtime", false)]
        private bool m_EditorDummyBool;
        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton), PropertyOrder(-1)]
        public bool Prepare() {
            bool isDirty = false;
            
            if (BeatmapTitle == null) {
                isDirty |= transform.TryFindFirstComponent(out BeatmapTitle, "Text_BeatmapTitle");
            }
            if (AuthorsLabel == null) {
                isDirty |= transform.TryFindFirstComponent(out AuthorsLabel, "Text_Artists");
            }
            if (BackgroundImage == null) {
                isDirty |= transform.TryFindFirstComponent(out BackgroundImage, "Button_Background");
            }
            if (BackgroundButton == null) {
                isDirty |= transform.TryFindFirstComponent(out BackgroundButton, "Button_Background");
            }
            if (PlayButton == null) {
                isDirty |= transform.TryFindFirstComponent(out PlayButton, "Button_Play");
            }
            
            return isDirty;
        }
    }
#endif
    
    public partial class BeatmapCell : FancyScrollRectCell<BeatmapData, BeatmapScrollContext> {
        [field: SerializeField, FoldoutGroup("Scene")] public TMP_Text BeatmapTitle;
        [field: SerializeField, FoldoutGroup("Scene")] public TMP_Text AuthorsLabel;
        [field: SerializeField, FoldoutGroup("Scene")] public Image BackgroundImage;
        [field: SerializeField, FoldoutGroup("Scene")] public Button BackgroundButton;
        [field: SerializeField, FoldoutGroup("Scene")] public Button PlayButton;
        
        public override void Initialize() {
            BackgroundButton.onClick.AddListener(() => Context.OnCellClicked?.Invoke(this));
            PlayButton.onClick.AddListener(() => Context.OnPlayButtonClicked?.Invoke(this));
        }
        public override void UpdateContent(BeatmapData data) {
            BeatmapTitle.text = data.BeatmapTitle;
            AuthorsLabel.text = data.AuthorsLabel;
            BackgroundImage.sprite = data.BackgroundImage;
        }
        protected override void UpdatePosition(float normalizedPosition, float localPosition) {
            base.UpdatePosition(normalizedPosition, localPosition);
            var wave = Mathf.Sin(normalizedPosition * Mathf.PI * 2) * 65;
            transform.localPosition += Vector3.right * wave;
        }
    }
}