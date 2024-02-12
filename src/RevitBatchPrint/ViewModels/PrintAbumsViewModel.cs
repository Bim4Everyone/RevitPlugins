using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitBatchPrint.Models;

using Visibility = System.Windows.Visibility;

namespace RevitBatchPrint.ViewModels {
    internal class PrintAbumsViewModel : BaseViewModel {
        private ObservableCollection<PrintAlbumViewModel> _albums;
        private PrintSettingsViewModel _printSettings = new PrintSettingsViewModel();

        private string _printParamName;
        private string _errorText;
        private readonly RevitRepository _repository;

        private Visibility _visibilitySaveFile;
        private Visibility _showPrintParamSelect;
        private ObservableCollection<PrintAlbumViewModel> _selectedAlbums;

        public PrintAbumsViewModel(UIApplication uiApplication) {
            _repository = new RevitRepository(uiApplication);
            SelectedAlbums = new ObservableCollection<PrintAlbumViewModel>();
            PrintParamNames = new ObservableCollection<string>(_repository.GetPrintParamNames());
            PrintParamName = PrintParamNames.FirstOrDefault(item => RevitRepository.PrintParamNames.Contains(item));
            ShowPrintParamSelect = string.IsNullOrEmpty(PrintParamName)
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;

            RevitSaveCommand = new RelayCommand(RevitSave, CanRevitSave);
            RevitPrintCommand = new RelayCommand(RevitPrint, CanRevitPrint);

#if REVIT_2022_OR_GREATER
            VisibilitySaveFile = System.Windows.Visibility.Visible;
#else
            VisibilitySaveFile = System.Windows.Visibility.Collapsed;
#endif

            SetPrintConfig();
        }

        public string PrintParamName {
            get => _printParamName;
            set {
                this.RaiseAndSetIfChanged(ref _printParamName, value);
                if(!string.IsNullOrEmpty(PrintParamName)) {
                    List<(string, int)> printParamValues = _repository.GetPrintParamValues(PrintParamName);
                    Albums = new ObservableCollection<PrintAlbumViewModel>(
                        printParamValues.Select(item => new PrintAlbumViewModel(item.Item1) { Count = item.Item2 }));
                    SelectedAlbums = new ObservableCollection<PrintAlbumViewModel>();
                }
            }
        }

        public ObservableCollection<string> PrintParamNames { get; }


        public ICommand RevitSaveCommand { get; }
        public ICommand RevitPrintCommand { get; }

        public ObservableCollection<PrintAlbumViewModel> Albums {
            get => _albums;
            set => this.RaiseAndSetIfChanged(ref _albums, value);
        }

        public ObservableCollection<PrintAlbumViewModel> SelectedAlbums {
            get => _selectedAlbums;
            set => this.RaiseAndSetIfChanged(ref _selectedAlbums, value);
        }

        public PrintSettingsViewModel PrintSettings {
            get => _printSettings;
            set => this.RaiseAndSetIfChanged(ref _printSettings, value);
        }

        public Visibility ShowPrintParamSelect {
            get => _showPrintParamSelect;
            set => this.RaiseAndSetIfChanged(ref _showPrintParamSelect, value);
        }

        public Visibility VisibilitySaveFile {
            get => _visibilitySaveFile;
            set => this.RaiseAndSetIfChanged(ref _visibilitySaveFile, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public void RevitPrint(object p) {
            SavePrintConfig();

            var revitPrintErrors = new List<string>();
            foreach(var album in SelectedAlbums) {
                var revitPrint = new RevitPrint(_repository);
                revitPrint.PrinterName = _printSettings.PrinterName;
                revitPrint.FilterParamName = PrintParamName;
                revitPrint.FilterParamValue = album.Name;
                revitPrint.PrinterSettings = PrintSettings.GetPrinterSettings();

                revitPrint.Execute(SetupPrintParams);
                revitPrintErrors.AddRange(revitPrint.Errors);
            }

            if(revitPrintErrors.Count > 0) {
                TaskDialog.Show("Пакетная печать.",
                    Environment.NewLine + "- " + string.Join(Environment.NewLine + "- ", revitPrintErrors));
            } else {
                TaskDialog.Show("Пакетная печать.", "Готово!");
            }
        }

        public bool CanRevitPrint(object p) {
            if(string.IsNullOrEmpty(PrintParamName)) {
                ErrorText = "Не был выбран параметр комплекта чертежей.";
                return false;
            }

            if(SelectedAlbums.Count() == 0) {
                ErrorText = "Не был выбран комплект чертежей.";
                return false;
            }

            if(string.IsNullOrEmpty(PrintSettings.PrinterName)) {
                ErrorText = "Не был выбран принтер.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        public void RevitSave(object p) {
            SavePrintConfig();

#if REVIT_2022_OR_GREATER
            var revitPrintErrors = new List<string>();

            var revitPrint = new RevitPrint2022(_repository);
            revitPrint.Folder = Path.GetDirectoryName(PrintSettings.FileName);
            revitPrint.FilterParamName = PrintParamName;
            revitPrint.FilterParamValues = SelectedAlbums
                .Select(item => item.Name)
                .OrderBy(item => item)
                .ToArray();

            var exportOptions = new PDFExportOptions() {
                FileName = Path.GetFileNameWithoutExtension(PrintSettings.FileName),
                Combine = true,
                StopOnError = false,
                AlwaysUseRaster = false,
                ColorDepth = PrintSettings.ColorDepth,
                ExportQuality = PDFExportQualityType.DPI300,
                RasterQuality = PrintSettings.RasterQuality,
                PaperPlacement = PrintSettings.PaperPlacement,
                HideScopeBoxes = PrintSettings.HideScopeBoxes,
                ViewLinksInBlue = PrintSettings.ViewLinksinBlue,
                HideCropBoundaries = PrintSettings.HideCropBoundaries,
                HideReferencePlane = PrintSettings.HideReforWorkPlanes,
                MaskCoincidentLines = PrintSettings.MaskCoincidentLines,
                HideUnreferencedViewTags = PrintSettings.HideUnreferencedViewTags,
                ReplaceHalftoneWithThinLines = PrintSettings.ReplaceHalftoneWithThinLines,
                ZoomType = PrintSettings.ZoomType,
                ZoomPercentage = PrintSettings.Zoom,
                OriginOffsetX = PrintSettings.UserDefinedMarginX,
                OriginOffsetY = PrintSettings.UserDefinedMarginY,
            };

            revitPrint.Execute(exportOptions);
            revitPrintErrors.AddRange(revitPrint.Errors);
            
            if(revitPrintErrors.Count > 0) {
                TaskDialog.Show("Пакетная печать.", Environment.NewLine + "- " + string.Join(Environment.NewLine + "- ", revitPrintErrors));
            } else {
                TaskDialog.Show("Пакетная печать.", "Готово!");
            }
#endif
        }

        public bool CanRevitSave(object p) {
            
#if REVIT_2022_OR_GREATER
            if(string.IsNullOrEmpty(PrintParamName)) {
                return false;
            }
            
            if(SelectedAlbums.Count() == 0) {
                return false;
            }

            if(string.IsNullOrEmpty(PrintSettings.FileName)) {
                ErrorText = "Выберите файл сохранения pdf.";
                return false;
            }
#endif
            
            return true;
        }

        private void SetPrintConfig() {
            _printSettings.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                _repository.Document.Title + ".pdf");

            var printSettingsConfig = PrintConfig
                .GetPrintConfig()
                .GetSettings(_repository.Document);

            if(printSettingsConfig != null) {
                _printSettings.PrinterName = printSettingsConfig.PrinterName;
                if(PrintParamNames.Contains(printSettingsConfig.PrintParamName)) {
                    PrintParamName = printSettingsConfig.PrintParamName;

                    if(Albums != null) {
                        foreach(var album in Albums) {
                            album.IsSelected =
                                printSettingsConfig.SelectedAlbums.Any(item => item?.Equals(album.Name) == true);
                        }
                        SelectedAlbums = new ObservableCollection<PrintAlbumViewModel>(Albums.Where(item => item.IsSelected));
                    }
                }

                _printSettings.Zoom = printSettingsConfig.Zoom;
                _printSettings.ZoomType = printSettingsConfig.ZoomType;

                _printSettings.ColorDepth = printSettingsConfig.ColorDepth;
                _printSettings.RasterQuality = printSettingsConfig.RasterQuality;

                _printSettings.PaperPlacement = printSettingsConfig.PaperPlacement;
                _printSettings.MarginType = printSettingsConfig.MarginType;
                _printSettings.UserDefinedMarginX = printSettingsConfig.UserDefinedMarginX;
                _printSettings.UserDefinedMarginY = printSettingsConfig.UserDefinedMarginY;

                _printSettings.HiddenLineViews = printSettingsConfig.HiddenLineViews;
                _printSettings.MaskCoincidentLines = printSettingsConfig.MaskCoincidentLines;
                _printSettings.ViewLinksinBlue = printSettingsConfig.ViewLinksinBlue;
                _printSettings.HideCropBoundaries = printSettingsConfig.HideCropBoundaries;
                _printSettings.HideReforWorkPlanes = printSettingsConfig.HideReforWorkPlanes;
                _printSettings.HideScopeBoxes = printSettingsConfig.HideScopeBoxes;
                _printSettings.HideUnreferencedViewTags = printSettingsConfig.HideUnreferencedViewTags;
                _printSettings.ReplaceHalftoneWithThinLines = printSettingsConfig.ReplaceHalftoneWithThinLines;

                if(!string.IsNullOrEmpty(printSettingsConfig.FolderName)) {
                    _printSettings.FileName =
                        Path.Combine(printSettingsConfig.FolderName, _repository.Document.Title + ".pdf");
                }
            }
        }

        private void SavePrintConfig() {
            var printConfig = PrintConfig.GetPrintConfig();
            var printSettingsConfig = printConfig.GetSettings(_repository.Document);
            if(printSettingsConfig == null) {
                printSettingsConfig = printConfig.AddSettings(_repository.Document);
            }

            printSettingsConfig.PrinterName = _printSettings.PrinterName;
            printSettingsConfig.PrintParamName = PrintParamName;
            printSettingsConfig.SelectedAlbums =
                SelectedAlbums.Select(item => item.Name).ToList();

            printSettingsConfig.Zoom = _printSettings.Zoom;
            printSettingsConfig.ZoomType = _printSettings.ZoomType;

            printSettingsConfig.ColorDepth = _printSettings.ColorDepth;
            printSettingsConfig.RasterQuality = _printSettings.RasterQuality;

            printSettingsConfig.PaperPlacement = _printSettings.PaperPlacement;
            printSettingsConfig.MarginType = _printSettings.MarginType;
            printSettingsConfig.UserDefinedMarginX = _printSettings.UserDefinedMarginX;
            printSettingsConfig.UserDefinedMarginY = _printSettings.UserDefinedMarginY;

            printSettingsConfig.HiddenLineViews = _printSettings.HiddenLineViews;
            printSettingsConfig.ViewLinksinBlue = _printSettings.ViewLinksinBlue;
            printSettingsConfig.HideCropBoundaries = _printSettings.HideCropBoundaries;
            printSettingsConfig.HideReforWorkPlanes = _printSettings.HideReforWorkPlanes;
            printSettingsConfig.HideScopeBoxes = _printSettings.HideScopeBoxes;
            printSettingsConfig.HideUnreferencedViewTags = _printSettings.HideUnreferencedViewTags;
            printSettingsConfig.MaskCoincidentLines = _printSettings.MaskCoincidentLines;
            printSettingsConfig.ReplaceHalftoneWithThinLines = _printSettings.ReplaceHalftoneWithThinLines;

            if(!string.IsNullOrEmpty(_printSettings.FileName)) {
                printSettingsConfig.FolderName = Path.GetDirectoryName(_printSettings.FileName);
            }

            printConfig.SaveProjectConfig();
        }

        private void SetupPrintParams(PrintParameters printParameters) {
            printParameters.PaperPlacement = PrintSettings.PaperPlacement;
            if(PrintSettings.PaperPlacement == PaperPlacementType.Margins) {
                printParameters.MarginType = PrintSettings.MarginType;
                if(PrintSettings.MarginType == MarginType.UserDefined) {
#if REVIT_2021_OR_LESS
                    printParameters.UserDefinedMarginX = PrintSettings.UserDefinedMarginX;
                    printParameters.UserDefinedMarginY = PrintSettings.UserDefinedMarginY;
#else
                    printParameters.OriginOffsetX = PrintSettings.UserDefinedMarginX;
                    printParameters.OriginOffsetY = PrintSettings.UserDefinedMarginY;
#endif
                }
            }

            printParameters.ZoomType = PrintSettings.ZoomType;
            if(PrintSettings.ZoomType == ZoomType.Zoom) {
                printParameters.Zoom = PrintSettings.Zoom;
            }

            printParameters.ColorDepth = PrintSettings.ColorDepth;
            printParameters.RasterQuality = PrintSettings.RasterQuality;

            printParameters.HiddenLineViews = PrintSettings.HiddenLineViews;
            printParameters.HideCropBoundaries = PrintSettings.HideCropBoundaries;
            printParameters.HideReforWorkPlanes = PrintSettings.HideReforWorkPlanes;
            printParameters.HideScopeBoxes = PrintSettings.HideScopeBoxes;
            printParameters.HideUnreferencedViewTags = PrintSettings.HideUnreferencedViewTags;
            printParameters.MaskCoincidentLines = PrintSettings.MaskCoincidentLines;
            printParameters.ReplaceHalftoneWithThinLines = PrintSettings.ReplaceHalftoneWithThinLines;
        }
    }
}