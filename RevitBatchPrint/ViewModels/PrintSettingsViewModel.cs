using System.Collections.Generic;
using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitBatchPrint.ViewModels {
    public class PrintSettingsViewModel : BaseViewModel {
        private string _printerName;

        private PaperPlacementType _paperPlacement = PaperPlacementType.Center;
        private MarginType _marginType = MarginType.NoMargin;
        private ZoomType _zoomType = ZoomType.FitToPage;
        private HiddenLineViewsType _hiddenLineViews = HiddenLineViewsType.VectorProcessing;
        private ColorDepthType _colorDepth = ColorDepthType.Color;
        private RasterQualityType _rasterQuality = RasterQualityType.Presentation;

        private int _zoom = 1;
        private double _userDefinedMarginX;
        private double _userDefinedMarginY;
        private bool _maskCoincidentLines;
        private bool _viewLinksinBlue;
        private bool _hideReforWorkPlanes;
        private bool _hideScopeBoxes;
        private bool _hideUnreferencedViewTags;
        private bool _hideCropBoundaries;
        private bool _replaceHalftoneWithThinLines;

        public string PrinterName {
            get => _printerName;
            set => this.RaiseAndSetIfChanged(ref _printerName, value);
        }

        public ObservableCollection<string> PrinterNames { get; }

        public PaperPlacementType PaperPlacement {
            get => _paperPlacement;
            set => this.RaiseAndSetIfChanged(ref _paperPlacement, value);
        }
        public MarginType MarginType {
            get => _marginType;
            set => this.RaiseAndSetIfChanged(ref _marginType, value);
        }
        public ZoomType ZoomType {
            get => _zoomType;
            set => this.RaiseAndSetIfChanged(ref _zoomType, value);
        }
        public HiddenLineViewsType HiddenLineViews {
            get => _hiddenLineViews;
            set => this.RaiseAndSetIfChanged(ref _hiddenLineViews, value);
        }
        public ColorDepthType ColorDepth {
            get => _colorDepth;
            set => this.RaiseAndSetIfChanged(ref _colorDepth, value);
        }
        public RasterQualityType RasterQuality {
            get => _rasterQuality;
            set => this.RaiseAndSetIfChanged(ref _rasterQuality, value);
        }
        public int Zoom {
            get => _zoom;
            set => this.RaiseAndSetIfChanged(ref _zoom, value);
        }
        public double UserDefinedMarginX {
            get => _userDefinedMarginX;
            set => this.RaiseAndSetIfChanged(ref _userDefinedMarginX, value);
        }
        public double UserDefinedMarginY {
            get => _userDefinedMarginY;
            set => this.RaiseAndSetIfChanged(ref _userDefinedMarginY, value);
        }

        public bool MaskCoincidentLines {
            get => _maskCoincidentLines;
            set => this.RaiseAndSetIfChanged(ref _maskCoincidentLines, value);
        }

        public bool ViewLinksinBlue {
            get => _viewLinksinBlue;
            set => this.RaiseAndSetIfChanged(ref _viewLinksinBlue, value);
        }

        public bool HideReforWorkPlanes {
            get => _hideReforWorkPlanes;
            set => this.RaiseAndSetIfChanged(ref _hideReforWorkPlanes, value);
        }

        public bool HideUnreferencedViewTags {
            get => _hideUnreferencedViewTags;
            set => this.RaiseAndSetIfChanged(ref _hideUnreferencedViewTags, value);
        }

        public bool HideScopeBoxes {
            get => _hideScopeBoxes;
            set => this.RaiseAndSetIfChanged(ref _hideScopeBoxes, value);
        }

        public bool HideCropBoundaries {
            get => _hideCropBoundaries;
            set => this.RaiseAndSetIfChanged(ref _hideCropBoundaries, value);
        }

        public bool ReplaceHalftoneWithThinLines {
            get => _replaceHalftoneWithThinLines;
            set => this.RaiseAndSetIfChanged(ref _replaceHalftoneWithThinLines, value);
        }
    }
}
