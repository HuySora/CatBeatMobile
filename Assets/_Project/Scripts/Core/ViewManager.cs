using Sirenix.OdinInspector;
using UnityEngine;

namespace VTBeat {
    public class ViewManager {
        [field: Title("Scene")]
        [field: SerializeField] public UISheet MainSheet { get; private set; }
        [field: SerializeField] public UISheet OverlaySheet { get; private set; }
        [field: SerializeField] public UIStack PopupStack { get; private set; }
    }
}