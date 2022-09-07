using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.RevitClashReport;

namespace RevitClashDetective.ViewModels.Navigator {

    internal class ReportsViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private bool _openFromClashDetector;
        private string _message;
        private ReportViewModel _selectedFile;
        private DispatcherTimer _timer;
        private List<ReportViewModel> _reports;

        public ReportsViewModel(RevitRepository revitRepository, string selectedFile = null) {
            _revitRepository = revitRepository;

            if(selectedFile == null) {
                InitializeFiles();
            } else {
                InitializeFiles(selectedFile);
            }

            InitializeTimer();

            SaveCommand = new RelayCommand(SaveConfig, CanSaveConfig);
            OpenClashDetectorCommand = new RelayCommand(OpenClashDetector, p => OpenFromClashDetector);
            LoadCommand = new RelayCommand(Load);
        }

        public ICommand SaveCommand { get; }
        public ICommand OpenClashDetectorCommand { get; }
        public ICommand LoadCommand { get; }

        public List<ReportViewModel> Reports {
            get => _reports;
            set => this.RaiseAndSetIfChanged(ref _reports, value);
        }

        public bool IsColumnVisible => Reports != null;

        public bool OpenFromClashDetector {
            get => _openFromClashDetector;
            set => this.RaiseAndSetIfChanged(ref _openFromClashDetector, value);
        }

        public ReportViewModel SelectedReport {
            get => _selectedFile;
            set => this.RaiseAndSetIfChanged(ref _selectedFile, value);
        }


        public string Message {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        private void InitializeFiles(string selectedFile) {
            var profilePath = RevitRepository.ProfilePath;
            Reports = Directory.GetFiles(Path.Combine(profilePath, ModuleEnvironment.RevitVersion, nameof(RevitClashDetective), _revitRepository.GetObjectName()))
                .Select(path => new ReportViewModel(_revitRepository, Path.GetFileNameWithoutExtension(path)))
                .Where(item => item.Name.Equals(selectedFile, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
            SelectedReport = Reports.FirstOrDefault();
        }

        private void InitializeFiles() {
            var profilePath = RevitRepository.ProfilePath;
            var path = Path.Combine(profilePath, ModuleEnvironment.RevitVersion, nameof(RevitClashDetective), _revitRepository.GetObjectName());
            if(Directory.Exists(path)) {
                Reports = Directory.GetFiles(path)
                                   .Select(item => new ReportViewModel(_revitRepository, Path.GetFileNameWithoutExtension(item)))
                                   .ToList();
                SelectedReport = Reports.FirstOrDefault();
            }
        }

        private void SaveConfig(object p) {
            SelectedReport.Save();
            Message = "Файл успешно сохранен";
            RefreshMessage();
        }

        private bool CanSaveConfig(object p) {
            return SelectedReport != null;
        }

        private void OpenClashDetector(object p) {
            void action() {
                var command = new DetectiveClashesCommand();
                command.ExecuteCommand(_revitRepository.UiApplication);
            }
            _revitRepository.DoAction(action);
        }

        private void InitializeTimer() {
            _timer = new DispatcherTimer {
                Interval = new TimeSpan(0, 0, 0, 3)
            };
            _timer.Tick += (s, a) => { Message = null; _timer.Stop(); };
        }

        private void RefreshMessage() {
            _timer.Start();
        }

        private void Load(object p) {
            var openWindow = GetPlatformService<IOpenFileDialogService>();
            openWindow.Filter = "AutodeskClashReport (*.html)|*.html|PluginClashReport (*.json)|*.json";
            if(!openWindow.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))) {
                throw new OperationCanceledException();
            }

            InitializeClashes(openWindow.File.FullName);
        }

        private void InitializeClashes(string path) {
            var name = Path.GetFileNameWithoutExtension(path);
            var clashes = ReportLoader.GetClashes(_revitRepository, path)
                                      .ToList();
            var report = new ReportViewModel(_revitRepository, name, clashes);

            Reports = new NameResolver<ReportViewModel>(Reports, new[] { report }).GetCollection()
                                                                                  .ToList();
            SelectedReport = report;
        }
    }
}
