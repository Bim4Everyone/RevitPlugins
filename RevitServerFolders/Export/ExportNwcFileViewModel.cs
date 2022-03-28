using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

using dosymep.WPF.Commands;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace RevitServerFolders.Export {
    public class ExportNwcFileViewModel : INotifyPropertyChanged, IDataErrorInfo {
        private bool _withRooms;
        private bool _withSubFolders;
        private string _sourceNwcFolder;
        private string _targetNwcFolder;
        private bool _cleanTargetNwcFolder;
        private bool _withLinkedFiles;

        public ExportNwcFileViewModel() { }

        public ExportNwcFileViewModel(ExportNwcFileConfig exportNwcFileConfig) {
            WithRooms = exportNwcFileConfig.WithRooms;
            WithSubFolders = exportNwcFileConfig.WithSubFolders;
            WithLinkedFiles = exportNwcFileConfig.WithLinkedFiles;
            SourceNwcFolder = exportNwcFileConfig.SourceNwcFolder;
            TargetNwcFolder = exportNwcFileConfig.TargetNwcFolder;
            CleanTargetNwcFolder = exportNwcFileConfig.CleanTargetNwcFolder;

            SelectSourceNwcFolderCommand = new RelayCommand(SelectSourceNwcFolder);
            SelectTargetNwcFolderCommand = new RelayCommand(SelectTargetNwcFolder);
        }
        public bool WithRooms{
            get => _withRooms;
            set {
                _withRooms = value;
                OnPropertyChanged(nameof(WithRooms));
            }
        }
        public bool WithSubFolders {
            get => _withSubFolders;
            set {
                _withSubFolders = value;
                OnPropertyChanged(nameof(WithSubFolders));
            }
        }
        public string SourceNwcFolder {
            get => _sourceNwcFolder;
            set {
                _sourceNwcFolder = value;
                OnPropertyChanged(nameof(SourceNwcFolder));
            }
        }
        public string TargetNwcFolder {
            get => _targetNwcFolder;
            set {
                _targetNwcFolder = value;
                OnPropertyChanged(nameof(TargetNwcFolder));
            }
        }
        public bool CleanTargetNwcFolder {
            get => _cleanTargetNwcFolder;
            set {
                _cleanTargetNwcFolder = value;
                OnPropertyChanged(nameof(CleanTargetNwcFolder));
            }
        }
        
        public bool WithLinkedFiles {
            get => _withLinkedFiles;
            set {
                _withLinkedFiles = value;
                OnPropertyChanged(nameof(WithLinkedFiles));
            }
        }

        public ICommand SelectSourceNwcFolderCommand { get; set; }
        public ICommand SelectTargetNwcFolderCommand { get; set; }

        private void SelectSourceNwcFolder(object p) {
            using(var dialog = new CommonOpenFileDialog()) {
                dialog.IsFolderPicker = true;
                dialog.Title = "Папка RVT-файлов";
                dialog.InitialDirectory = SourceNwcFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                    SourceNwcFolder = dialog.FileName;
                }
            }
        }

        private void SelectTargetNwcFolder(object p) {
            using(var dialog = new CommonOpenFileDialog()) {
                dialog.IsFolderPicker = true;
                dialog.Title = "Папка сохранения NWC-файлов";
                dialog.InitialDirectory = TargetNwcFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                    TargetNwcFolder = dialog.FileName;
                }
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IDataErrorInfo
        private string _error;
        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();

        public bool IsValid {
            get { return string.IsNullOrEmpty(Error); }
        }

        public string Error {
            get => _error;
            private set {
                if(!string.Equals(_error, value)) {
                    _error = value;
                    OnPropertyChanged(nameof(Error));
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        public string this[string columnName] {
            get { return Validate(columnName); }
        }

        private string Validate(string propertyName) {
            string error = null;
            switch(propertyName) {
                case nameof(SourceNwcFolder):
                error = string.IsNullOrEmpty(SourceNwcFolder) ? "Выберите папку с файлами Revit!" : null;
                break;
                case nameof(TargetNwcFolder):
                error = string.IsNullOrEmpty(TargetNwcFolder) ? "Выберите папку сохранения NWC-файлов!" : null;
                break;
            }

            _errors[propertyName] = error;
            Error = _errors.Values.FirstOrDefault(item => !string.IsNullOrEmpty(item));
           
            return error;
        }

        #endregion
    }
}
