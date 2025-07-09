using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitBatchPrint.Models;

namespace RevitBatchPrint.ViewModels;

internal sealed class PrintOptionsViewModel : BaseViewModel {
    private string _filePath;
    private string _printerName;
    private ObservableCollection<string> _printerNames;

    private int _zoom = 1;
    private ZoomType _zoomType;

    private MarginType _marginType;
    private double _originOffsetX;
    private double _originOffsetY;

    private HiddenLineViewsType _hiddenLineViews;
    private ColorDepthType _colorDepth;
    private RasterQualityType _rasterQuality;
    private PaperPlacementType _paperPlacement;

    private bool _maskCoincidentLines;
    private bool _viewLinksInBlue;
    private bool _hideReferencePlane;
    private bool _hideUnreferencedViewTags;
    private bool _hideScopeBoxes;
    private bool _hideCropBoundaries;
    private bool _replaceHalftoneWithThinLines;

    public PrintOptionsViewModel(IEnumerable<string> printerNames, PrintOptions printOptions) {
        PrinterNames = new  ObservableCollection<string>(printerNames);
        
        FilePath = printOptions.FilePath;
        PrinterName = PrinterNames
                          .FirstOrDefault(item => printOptions.PrinterName.Equals(item))
                      ?? PrinterNames.FirstOrDefault();

        Zoom = printOptions.Zoom;
        ZoomType = printOptions.ZoomType;

        MarginType = printOptions.MarginType;
        OriginOffsetX = printOptions.OriginOffsetX;
        OriginOffsetY = printOptions.OriginOffsetY;

        HiddenLineViews = printOptions.HiddenLineViews;
        ColorDepth = printOptions.ColorDepth;
        RasterQuality = printOptions.RasterQuality;
        PaperPlacement = printOptions.PaperPlacement;

        MaskCoincidentLines = printOptions.MaskCoincidentLines;
        ViewLinksInBlue = printOptions.ViewLinksInBlue;
        HideReferencePlane = printOptions.HideReferencePlane;
        HideUnreferencedViewTags = printOptions.HideUnreferencedViewTags;
        HideScopeBoxes = printOptions.HideScopeBoxes;
        HideCropBoundaries = printOptions.HideCropBoundaries;
        ReplaceHalftoneWithThinLines = printOptions.ReplaceHalftoneWithThinLines;
    }

    public string FilePath {
        get => _filePath;
        set => this.RaiseAndSetIfChanged(ref _filePath, value);
    }

    public string PrinterName {
        get => _printerName;
        set => this.RaiseAndSetIfChanged(ref _printerName, value);
    }

    public ObservableCollection<string> PrinterNames {
        get => _printerNames;
        set => this.RaiseAndSetIfChanged(ref _printerNames, value);
    }

    public int Zoom {
        get => _zoom;
        set => this.RaiseAndSetIfChanged(ref _zoom, value);
    }

    public ZoomType ZoomType {
        get => _zoomType;
        set => this.RaiseAndSetIfChanged(ref _zoomType, value);
    }

    public MarginType MarginType {
        get => _marginType;
        set => this.RaiseAndSetIfChanged(ref _marginType, value);
    }

    public double OriginOffsetX {
        get => _originOffsetX;
        set => this.RaiseAndSetIfChanged(ref _originOffsetX, value);
    }

    public double OriginOffsetY {
        get => _originOffsetY;
        set => this.RaiseAndSetIfChanged(ref _originOffsetY, value);
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

    public PaperPlacementType PaperPlacement {
        get => _paperPlacement;
        set => this.RaiseAndSetIfChanged(ref _paperPlacement, value);
    }

    public bool MaskCoincidentLines {
        get => _maskCoincidentLines;
        set => this.RaiseAndSetIfChanged(ref _maskCoincidentLines, value);
    }

    public bool ViewLinksInBlue {
        get => _viewLinksInBlue;
        set => this.RaiseAndSetIfChanged(ref _viewLinksInBlue, value);
    }

    public bool HideReferencePlane {
        get => _hideReferencePlane;
        set => this.RaiseAndSetIfChanged(ref _hideReferencePlane, value);
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

    public PrintOptions CreatePrintOptions() {
        return new PrintOptions() {
            FilePath = FilePath,
            PrinterName = PrinterName,
            Zoom = Zoom,
            ZoomType = ZoomType,
            MarginType = MarginType,
            OriginOffsetX = OriginOffsetX,
            OriginOffsetY = OriginOffsetY,
            HiddenLineViews = HiddenLineViews,
            ColorDepth = ColorDepth,
            RasterQuality = RasterQuality,
            PaperPlacement = PaperPlacement,
            MaskCoincidentLines = MaskCoincidentLines,
            ViewLinksInBlue = ViewLinksInBlue,
            HideReferencePlane = HideReferencePlane,
            HideUnreferencedViewTags = HideUnreferencedViewTags,
            HideScopeBoxes = HideScopeBoxes,
            HideCropBoundaries = HideCropBoundaries,
            ReplaceHalftoneWithThinLines = ReplaceHalftoneWithThinLines
        };
    }
}
