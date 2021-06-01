using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Input;

namespace RevitServerFolders {
    public class ExportNwcFileConfig {
        public bool WithSubFolders { get; set; }

        public string SourceNwcFolder { get; set; }
        public string TargetNwcFolder { get; set; }

        public bool CleanTargetNwcFolder { get; set; }

        private static string GetConfigPath() {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dosymep", "RevitServerFolders", "ExportNwcFileConfig.json");
        }

        public static ExportNwcFileConfig GetExportNwcFileConfig() {
            if(File.Exists(GetConfigPath())) {
                return JsonSerializer.Deserialize<ExportNwcFileConfig>(File.ReadAllText(GetConfigPath()));
            }

            return new ExportNwcFileConfig();
        }

        public static void SaveExportNwcFileConfig(ExportNwcFileConfig exportNwcFileConfig) {
            Directory.CreateDirectory(Path.GetDirectoryName(GetConfigPath()));
            File.WriteAllText(GetConfigPath(), JsonSerializer.Serialize(exportNwcFileConfig, typeof(ExportNwcFileConfig)));
        }
    }

    public class ExportNwcFileViewModel : INotifyPropertyChanged, IDataErrorInfo {
        private bool _withSubFolders;
        private string _sourceNwcFolder;
        private string _targetNwcFolder;
        private bool _cleanTargetNwcFolder;

        public ExportNwcFileViewModel() { }

        public ExportNwcFileViewModel(ExportNwcFileConfig exportNwcFileConfig) {
            WithSubFolders = exportNwcFileConfig.WithSubFolders;
            SourceNwcFolder = exportNwcFileConfig.SourceNwcFolder;
            TargetNwcFolder = exportNwcFileConfig.TargetNwcFolder;
            CleanTargetNwcFolder = exportNwcFileConfig.CleanTargetNwcFolder;

            SelectSourceNwcFolderCommand = new RelayCommand(SelectSourceNwcFolder);
            SelectTargetNwcFolderCommand = new RelayCommand(SelectTargetNwcFolder);
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

        public ICommand SelectSourceNwcFolderCommand { get; set; }
        public ICommand SelectTargetNwcFolderCommand { get; set; }

        private void SelectSourceNwcFolder(object p) {
            using(var dialog = new FolderBrowserDialog()) {
                dialog.RootFolder = Environment.SpecialFolder.Desktop;
                dialog.SelectedPath = SourceNwcFolder;
                if(dialog.ShowDialog() == DialogResult.OK) {
                    SourceNwcFolder = dialog.SelectedPath;
                }
            }
        }

        private void SelectTargetNwcFolder(object p) {
            using(var dialog = new FolderBrowserDialog()) {
                dialog.RootFolder = Environment.SpecialFolder.Desktop;
                dialog.SelectedPath = TargetNwcFolder;
                if(dialog.ShowDialog() == DialogResult.OK) {
                    TargetNwcFolder = dialog.SelectedPath;
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
