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

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitBatchPrint.Models;

namespace RevitBatchPrint.ViewModels {
    internal class PrintAbumsViewModel : BaseViewModel {
        private PrintAlbumViewModel _selectedAlbum;
        private ObservableCollection<PrintAlbumViewModel> _albums;
        private PrintSettingsViewModel _printSettings = new PrintSettingsViewModel();

        private System.Windows.Visibility _showPrintParamSelect;
        private string _printParamName;
        private string _errorText;
        private readonly RevitRepository _repository;

        public PrintAbumsViewModel(UIApplication uiApplication) {
            _repository = new RevitRepository(uiApplication);

            PrintParamNames = new ObservableCollection<string>(_repository.GetPrintParamNames());
            PrintParamName = PrintParamNames.FirstOrDefault(item => RevitRepository.PrintParamNames.Contains(item));
            ShowPrintParamSelect = string.IsNullOrEmpty(PrintParamName) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            RevitPrintCommand = new RelayCommand(RevitPrint, CanRevitPrint);

            SetPrintConfig();
        }

        public string PrintParamName {
            get => _printParamName;
            set {
                this.RaiseAndSetIfChanged(ref _printParamName, value);
                if(!string.IsNullOrEmpty(PrintParamName)) {
                    List<(string, int)> printParamValues = _repository.GetPrintParamValues(PrintParamName);
                    Albums = new ObservableCollection<PrintAlbumViewModel>(printParamValues.Select(item => new PrintAlbumViewModel() { Name = item.Item1, Count = item.Item2 }));
                    SelectedAlbum = Albums.FirstOrDefault();
                }
            }
        }

        public ObservableCollection<string> PrintParamNames { get; }

        public ICommand RevitPrintCommand { get; }

        public PrintAlbumViewModel SelectedAlbum {
            get => _selectedAlbum;
            set => this.RaiseAndSetIfChanged(ref _selectedAlbum, value);
        }

        public ObservableCollection<PrintAlbumViewModel> Albums {
            get => _albums;
            set => this.RaiseAndSetIfChanged(ref _albums, value);
        }

        public PrintSettingsViewModel PrintSettings {
            get => _printSettings;
            set => this.RaiseAndSetIfChanged(ref _printSettings, value);
        }

        public System.Windows.Visibility ShowPrintParamSelect {
            get => _showPrintParamSelect;
            set => this.RaiseAndSetIfChanged(ref _showPrintParamSelect, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public void RevitPrint(object p) {
            SavePrintConfig();

            var revitPrint = new RevitPrint(_repository);
            revitPrint.PrinterName = RevitRepository.DefaultPrinterName;
            revitPrint.FilterParamName = PrintParamName;
            revitPrint.FilterParamValue = SelectedAlbum.Name;
            revitPrint.PrinterSettings = PrintSettings.GetPrinterSettings();

            revitPrint.Execute(SetupPrintParams);

            if(revitPrint.Errors.Count > 0) {
                TaskDialog.Show("Пакетная печать.", string.Join(Environment.NewLine + "- ", revitPrint.Errors));
            } else {
                TaskDialog.Show("Пакетная печать.", "Готово!");
            }
        }

        private void SetPrintConfig() {
            var printSettingsConfig = PrintConfig.GetConfig().GetPrintSettingsConfig(GetDocumentName());
            if(printSettingsConfig != null) {
                _printSettings.PrinterName = printSettingsConfig.PrinterName;
                PrintParamName = printSettingsConfig.PrintParamName;
                SelectedAlbum = Albums?.FirstOrDefault(item => item.Name.Equals(printSettingsConfig.SelectedAlbum)) ?? SelectedAlbum;

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
            }
        }

        private void SavePrintConfig() {
            var printConfig = PrintConfig.GetConfig();
            var printSettingsConfig = printConfig.GetPrintSettingsConfig(Path.GetFileName(_repository.Document.PathName));
            if(printSettingsConfig == null) {
                printSettingsConfig = new PrintSettingsConfig();
                printConfig.AddPrintSettings(printSettingsConfig);
            }

            printSettingsConfig.DocumentName = GetDocumentName();
            printSettingsConfig.PrinterName = _printSettings.PrinterName;
            printSettingsConfig.PrintParamName = PrintParamName;
            printSettingsConfig.SelectedAlbum = SelectedAlbum?.Name;

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

            PrintConfig.SaveConfig(printConfig);
        }
        private string GetDocumentName() {
            return Path.GetFileName(_repository.Document.PathName);
        }

        public bool CanRevitPrint(object p) {
            if(string.IsNullOrEmpty(PrintParamName)) {
                ErrorText = "Не был выбран параметр комплекта чертежей.";
                return false;
            }

            if(SelectedAlbum is null) {
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

        private void SetupPrintParams(PrintParameters printParameters) {
            printParameters.PaperPlacement = PrintSettings.PaperPlacement;
            if(PrintSettings.PaperPlacement == PaperPlacementType.Margins) {
                printParameters.MarginType = PrintSettings.MarginType;
                if(PrintSettings.MarginType == MarginType.UserDefined) {
                    printParameters.UserDefinedMarginX = PrintSettings.UserDefinedMarginX;
                    printParameters.UserDefinedMarginY = PrintSettings.UserDefinedMarginY;
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