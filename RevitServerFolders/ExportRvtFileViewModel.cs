using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Input;

using dosymep.Revit.ServerClient;

namespace RevitServerFolders {
    public class ExportRvtFileConfig {
        public string ServerName { get; set; }

        public bool WithNwcFiles { get; set; }
        public bool WithSubFolders { get; set; }

        public string SourceRvtFolder { get; set; }
        public string TargetRvtFolder { get; set; }
        public string TargetNwcFolder { get; set; }

        public bool CleanTargetRvtFolder { get; set; }
        public bool CleanTargetNwcFolder { get; set; }

        private static string GetConfigPath() {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dosymep", "RevitServerFolders", "ExportRvtFileConfig.json");
        }

        public static ExportRvtFileConfig GetExportRvtFileConfig() {
            if(File.Exists(GetConfigPath())) {
                return JsonSerializer.Deserialize<ExportRvtFileConfig>(File.ReadAllText(GetConfigPath()));
            }

            return new ExportRvtFileConfig();
        }

        public static void SaveExportRvtFileConfig(ExportRvtFileConfig exportRvtFileConfig) {
            Directory.CreateDirectory(Path.GetDirectoryName(GetConfigPath()));
            File.WriteAllText(GetConfigPath(), JsonSerializer.Serialize(exportRvtFileConfig, typeof(ExportRvtFileConfig)));
        }
    }


    public class ExportRvtFileViewModel : INotifyPropertyChanged, IDataErrorInfo {
        private string _serverName;
        private List<string> _serverNames;

        private bool _withSubFolders;
        private string _sourceRvtFolder;
        private string _targetRvtFolder;

        private bool _withNwcFiles;
        private string _targetNwcFolder;

        private bool _cleanTargetRvtFolder;
        private bool _cleanTargetNwcFolder;
        private string _error;

        public ExportRvtFileViewModel() { }

        public ExportRvtFileViewModel(string revitVersion, ExportRvtFileConfig exportRvtFileConfig) {
            RevitVersion = revitVersion;
            ServerNames = File.ReadAllLines($@"C:\ProgramData\Autodesk\Revit Server {RevitVersion}\Config\RSN.ini").ToList();

            ServerName = exportRvtFileConfig.ServerName ?? ServerNames?.FirstOrDefault();
            WithSubFolders = exportRvtFileConfig.WithSubFolders;
            SourceRvtFolder = exportRvtFileConfig.SourceRvtFolder;
            TargetRvtFolder = exportRvtFileConfig.TargetRvtFolder;
            WithNwcFiles = exportRvtFileConfig.WithNwcFiles;
            TargetNwcFolder = exportRvtFileConfig.TargetNwcFolder;
            CleanTargetRvtFolder = exportRvtFileConfig.CleanTargetRvtFolder;
            CleanTargetNwcFolder = exportRvtFileConfig.CleanTargetNwcFolder;

            SelectSourceRvtFolderCommand = new RelayCommand(p => {
                var client = new RevitServerClientBuilder()
                    .SetServerName(ServerName)
                    .SetServerVersion(RevitVersion)
                    .UseJsonNetSerializer()
                    .Build();

                var viewModel = new RevitServerViewModel(client) { CurrentFolder = SourceRvtFolder };
                var selectWindow = new SelectServerRvtFolderWindow() { DataContext = viewModel };

                if(selectWindow.ShowDialog() == true) {
                    SourceRvtFolder = ((RevitServerViewModel) selectWindow.DataContext).CurrentFolder;
                }
            });

            SelectTargetRvtFolderCommand = new RelayCommand(SelectTargetRvtFolder);
            SelectTargetNwcFolderCommand = new RelayCommand(SelectTargetNwcFolder);
        }

        public string RevitVersion { get; }

        public string ServerName {
            get => _serverName;
            set {
                _serverName = value;
                OnPropertyChanged(nameof(ServerName));

                SourceRvtFolder = null;
            }
        }

        public List<string> ServerNames {
            get => _serverNames;
            set {
                _serverNames = value;
                OnPropertyChanged(nameof(ServerNames));
            }
        }

        public bool WithSubFolders {
            get => _withSubFolders;
            set {
                _withSubFolders = value;
                OnPropertyChanged(nameof(WithSubFolders));
            }
        }

        public string SourceRvtFolder {
            get => _sourceRvtFolder;
            set {
                _sourceRvtFolder = value;
                OnPropertyChanged(nameof(SourceRvtFolder));
            }
        }
        public string TargetRvtFolder {
            get => _targetRvtFolder;
            set {
                _targetRvtFolder = value;
                OnPropertyChanged(nameof(TargetRvtFolder));
            }
        }

        public bool CleanTargetRvtFolder {
            get => _cleanTargetRvtFolder;
            set {
                _cleanTargetRvtFolder = value;
                OnPropertyChanged(nameof(CleanTargetRvtFolder));
            }
        }

        public bool WithNwcFiles {
            get => _withNwcFiles;
            set {
                _withNwcFiles = value;
                OnPropertyChanged(nameof(WithNwcFiles));
                OnPropertyChanged(nameof(TargetNwcFolder));
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

        public ICommand SelectSourceRvtFolderCommand { get; set; }
        public ICommand SelectTargetRvtFolderCommand { get; set; }
        public ICommand SelectTargetNwcFolderCommand { get; set; }

        private void SelectTargetRvtFolder(object p) {
            using(var dialog = new FolderBrowserDialog()) {
                dialog.RootFolder = Environment.SpecialFolder.Desktop;
                dialog.SelectedPath = TargetRvtFolder;
                if(dialog.ShowDialog() == DialogResult.OK) {
                    TargetRvtFolder = dialog.SelectedPath;
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
                case nameof(ServerName):
                error = string.IsNullOrEmpty(ServerName) ? "Выберите имя сервера!" : null;
                break;
                case nameof(SourceRvtFolder):
                error = string.IsNullOrEmpty(SourceRvtFolder) ? "Выберите папку с файлами Revit!" : null;
                break;
                case nameof(TargetRvtFolder):
                error = string.IsNullOrEmpty(TargetRvtFolder) ? "Выберите папку сохранения открепленных файлов Revit!" : null;
                break;
                case nameof(TargetNwcFolder):
                error = string.IsNullOrEmpty(TargetNwcFolder) && WithNwcFiles ? "Выберите папку сохранения NWC-файлов!" : null;
                break;
            }

            _errors[propertyName] = error;
            Error = _errors.Values.FirstOrDefault(item => !string.IsNullOrEmpty(item));

            return error;
        }

        #endregion
    }
}