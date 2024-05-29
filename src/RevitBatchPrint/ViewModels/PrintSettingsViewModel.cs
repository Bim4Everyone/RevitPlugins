using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.Win32;

namespace RevitBatchPrint.ViewModels {
    internal class PrintSettingsViewModel : BaseViewModel {
        private string _printerName;

        private PaperPlacementType _paperPlacement = PaperPlacementType.Margins;
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
        private bool _hideReforWorkPlanes = true;
        private bool _hideScopeBoxes = true;
        private bool _hideUnreferencedViewTags;
        private bool _hideCropBoundaries = true;
        private bool _replaceHalftoneWithThinLines;
        private string _fileName;
        private readonly Models.Printing.PrintManager _printManager;

        public PrintSettingsViewModel() {
            _printManager = new Models.Printing.PrintManager();

            PrinterNames = new ObservableCollection<string>(_printManager.EnumPrinterNames());
            if(_printManager.HasPrinterName(Models.RevitRepository.DefaultPrinterName)) {
                PrinterName = Models.RevitRepository.DefaultPrinterName;
            }

            SaveFileCommand = new RelayCommand(SaveFile);
            VisibilitySelectFile = System.Windows.Visibility.Collapsed;

#if REVIT_2022_OR_GREATER
            VisibilitySelectFile = System.Windows.Visibility.Visible;
#endif
        }

        public string PrinterName {
            get => _printerName;
            set {
                if(_printManager.HasPrinterName(value)) {
                    this.RaiseAndSetIfChanged(ref _printerName, value);
                }
            }
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

        public ICommand SaveFileCommand { get; }
        public System.Windows.Visibility VisibilitySelectFile { get; }

        public string FileName {
            get => _fileName;
            set {
                try {
                    if(Directory.Exists(Path.GetDirectoryName(value))) {
                        this.RaiseAndSetIfChanged(ref _fileName, value);
                    }
                } catch {
                    TaskDialog.Show("Ошибка", "Переданный путь не является верным");
                }
            }
        }

        public Models.Printing.PrinterSettings GetPrinterSettings() {
            return _printManager.GetPrinterSettings(PrinterName);
        }

        private void SaveFile(object p) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = Path.GetFileName(FileName);
            openFileDialog.InitialDirectory = Path.GetDirectoryName(FileName);

            openFileDialog.CheckFileExists = false;
            openFileDialog.RestoreDirectory = true;

            openFileDialog.DefaultExt = ".pdf";
            openFileDialog.Filter = "pdf files (.pdf)|*.pdf";

            if(openFileDialog.ShowDialog() == true) {
                FileName = openFileDialog.FileName;
            }
        }
    }
}