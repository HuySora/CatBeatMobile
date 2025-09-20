using System;
using FancyScrollView;

namespace VTBeat.View {
    public class BeatmapScrollContext : FancyScrollRectContext {
        public int SelectedIndex = -1;
        public Action<BeatmapCell> OnCellClicked;
        public Action<BeatmapCell> OnPlayButtonClicked;
    }
}