﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitBatchPrint.Models;

namespace RevitBatchPrint.ViewModels {
    internal class PrintAbumsViewModel : BaseViewModel {
        private PrintAlbumViewModel _selectedAlbum;
        private ObservableCollection<PrintAlbumViewModel> _albums;
        private PrintSettingsViewModel _printSettings = new PrintSettingsViewModel();

        private Visibility _showPrintParamSelect;
        private string _printParamName;
        private string _errorText;
        private readonly RevitRepository _repository;

        public PrintAbumsViewModel(UIApplication uiApplication) {
            _repository = new RevitRepository(uiApplication);

            PrintParamNames = new ObservableCollection<string>(_repository.GetPrintParamNames());
            PrintParamName = PrintParamNames.FirstOrDefault(item => RevitRepository.PrintParamNames.Contains(item));
            ShowPrintParamSelect = string.IsNullOrEmpty(PrintParamName) ? Visibility.Visible : Visibility.Collapsed;

            RevitPrintCommand = new RelayCommand(RevitPrint, CanRevitPrint);
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

        public Visibility ShowPrintParamSelect {
            get => _showPrintParamSelect;
            set => this.RaiseAndSetIfChanged(ref _showPrintParamSelect, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public void RevitPrint(object p) {
            var revitPrint = new RevitPrint(_repository);
            revitPrint.PrinterName = RevitRepository.DefaultPrinterName;
            revitPrint.FilterParamName = PrintParamName;
            revitPrint.FilterParamValue = SelectedAlbum.Name;
            revitPrint.Execute();

            if(revitPrint.Errors.Count > 0) {
                TaskDialog.Show("Пакетная печать.", string.Join(Environment.NewLine + "- ", revitPrint.Errors));
            } else {
                TaskDialog.Show("Пакетная печать.", "Готово!");
            }
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

            return true;
        }
    }
}