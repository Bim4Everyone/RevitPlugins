using System.IO;

using Autodesk.Revit.DB;

namespace RevitBatchPrint.Models;

internal class PrintOptions {
    public string FilePath { get; set; }
    public string PrinterName { get; set; } = PluginSystemConfig.DefaultPrinterName;

    public int Zoom { get; set; } = 1;
    public ZoomType ZoomType { get; set; } = ZoomType.FitToPage;

    public MarginType MarginType { get; set; } = MarginType.NoMargin;
    public double OriginOffsetX { get; set; }
    public double OriginOffsetY { get; set; }

    public HiddenLineViewsType HiddenLineViews { get; set; } = HiddenLineViewsType.VectorProcessing;
    public ColorDepthType ColorDepth { get; set; } = ColorDepthType.Color;
    public RasterQualityType RasterQuality { get; set; } = RasterQualityType.High;
    public PaperPlacementType PaperPlacement { get; set; } = PaperPlacementType.Margins;

    public bool MaskCoincidentLines { get; set; }
    public bool ViewLinksInBlue { get; set; }
    public bool HideReferencePlane { get; set; } = true;
    public bool HideUnreferencedViewTags { get; set; }
    public bool HideScopeBoxes { get; set; } = true;
    public bool HideCropBoundaries { get; set; } = true;
    public bool ReplaceHalftoneWithThinLines { get; set; }

    public void Apply(PrintParameters printParameters) {
        printParameters.ZoomType = ZoomType;
        if(ZoomType == ZoomType.Zoom) {
            printParameters.Zoom = Zoom;
        }

        printParameters.PaperPlacement = PaperPlacement;
        if(PaperPlacement == PaperPlacementType.Margins) {
            printParameters.MarginType = MarginType;
            if(MarginType == MarginType.UserDefined) {
#if REVIT_2022_OR_GREATER
                printParameters.OriginOffsetX = OriginOffsetX;
                printParameters.OriginOffsetY = OriginOffsetY;
#else
                printParameters.UserDefinedMarginX = OriginOffsetX;
                printParameters.UserDefinedMarginY = OriginOffsetY;
#endif
            }
        }

        printParameters.HiddenLineViews = HiddenLineViews;
        printParameters.ColorDepth = ColorDepth;
        printParameters.RasterQuality = RasterQuality;

        printParameters.MaskCoincidentLines = MaskCoincidentLines;
        printParameters.ViewLinksinBlue = ViewLinksInBlue;
        printParameters.HideReforWorkPlanes = HideReferencePlane;
        printParameters.HideUnreferencedViewTags = HideUnreferencedViewTags;
        printParameters.HideScopeBoxes = HideScopeBoxes;
        printParameters.HideCropBoundaries = HideCropBoundaries;
        printParameters.ReplaceHalftoneWithThinLines = ReplaceHalftoneWithThinLines;
    }

#if REVIT_2022_OR_GREATER
    public PDFExportOptions CreateExportParams() {
        var exportOptions = new PDFExportOptions();

        exportOptions.FileName = Path.GetFileNameWithoutExtension(FilePath);

        exportOptions.Combine = true;
        exportOptions.StopOnError = false;
        exportOptions.AlwaysUseRaster = false;

        // Настройка качества растра и цвета
        exportOptions.RasterQuality = RasterQuality;
        exportOptions.ColorDepth = ColorDepth;
        exportOptions.ExportQuality = PDFExportQualityType.DPI300;
        exportOptions.PaperPlacement = PaperPlacement;

        // Настройка отображения элементов
        exportOptions.MaskCoincidentLines = MaskCoincidentLines;
        exportOptions.ViewLinksInBlue = ViewLinksInBlue;
        exportOptions.HideReferencePlane = HideReferencePlane;
        exportOptions.HideUnreferencedViewTags = HideUnreferencedViewTags;
        exportOptions.HideScopeBoxes = HideScopeBoxes;
        exportOptions.HideCropBoundaries = HideCropBoundaries;
        exportOptions.ReplaceHalftoneWithThinLines = ReplaceHalftoneWithThinLines;

        exportOptions.OriginOffsetX = OriginOffsetX;
        exportOptions.OriginOffsetY = OriginOffsetY;

        // Настройка масштабирования
        exportOptions.ZoomType = ZoomType;
        if(ZoomType == ZoomType.Zoom) {
            exportOptions.ZoomPercentage = Zoom * 100;
        }

        return exportOptions;
    }
#endif
}
