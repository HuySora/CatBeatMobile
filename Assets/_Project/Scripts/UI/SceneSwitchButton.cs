using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NavStack;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VTBeat {
    public partial class SceneSwitchButton : MonoBehaviour {
        [field: Title("Scene")]
        [field: SerializeField] public Button Button;
        [field: SerializeField] public TMP_Text ButtonLabel;
        [field: Title("Runtime")]
        [field: SerializeField] public RectTransform RectTransform;
        
        private Vector3 m_OriginalPosition;
        private Vector3 m_OriginalScale;
        
        private void Awake() {
            RectTransform = RectTransform == null ? GetComponent<RectTransform>() : null;
        }
        public void Initialize() {
            m_OriginalPosition = transform.localPosition;
            m_OriginalScale = transform.localScale;
        }
    }
    
    public partial class SceneSwitchButton : IPage {
        public async UniTask OnNavigatedFrom(NavigationContext context, CancellationToken cancellationToken = new()) {
            // Anim
            Vector3 offPos = m_OriginalPosition + Vector3.down * Screen.height;
            await RectTransform.DOLocalMove(offPos, 0.5f)
                .SetEase(Ease.InCubic)
                .AsyncWaitForCompletion().AsUniTask();
            // End
            gameObject.SetActive(false);
        }
        public async UniTask OnNavigatedTo(NavigationContext context, CancellationToken cancellationToken = new()) {
            // Start above the screen
            RectTransform.localPosition = m_OriginalPosition + Vector3.up * Screen.height;
            gameObject.SetActive(true);
            // Data
            var labelStr = context.Parameters["label"] as string ?? string.Empty;
            ButtonLabel.text = labelStr;
            // Anim
            await RectTransform.DOLocalMove(m_OriginalPosition, 0.5f)
                .SetEase(Ease.OutCubic)
                .AsyncWaitForCompletion().AsUniTask();
        }
    }
}