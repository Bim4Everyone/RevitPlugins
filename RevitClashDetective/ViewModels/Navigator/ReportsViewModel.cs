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
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.RevitClashReport;

namespace RevitClashDetective.ViewModels.Navigator {

    internal class ReportsViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private bool _openFromClashDetector;
        private ReportViewModel _selectedFile;
        private ObservableCollection<ReportViewModel> _reports;

        public ReportsViewModel(RevitRepository revitRepository, string selectedFile = null) {
            _revitRepository = revitRepository;
            Reports = new ObservableCollection<ReportViewModel>();

            if(selectedFile == null) {
                InitializeFiles();
            } else {
                InitializeFiles(selectedFile);
            }

            OpenClashDetectorCommand = new RelayCommand(OpenClashDetector, p => OpenFromClashDetector);
            LoadCommand = new RelayCommand(Load);
            DeleteCommand = new RelayCommand(Delete, CanDelete);
        }

        public ICommand OpenClashDetectorCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand DeleteCommand { get; }

        public ObservableCollection<ReportViewModel> Reports {
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


        private void InitializeFiles(string selectedFile) {
            var profilePath = RevitRepository.ProfilePath;
            Reports = new ObservableCollection<ReportViewModel>(Directory.GetFiles(Path.Combine(profilePath, ModuleEnvironment.RevitVersion, nameof(RevitClashDetective), _revitRepository.GetObjectName()))
                .Select(path => new ReportViewModel(_revitRepository, Path.GetFileNameWithoutExtension(path)))
                .Where(item => item.Name.Equals(selectedFile, StringComparison.CurrentCultureIgnoreCase)));
            SelectedReport = Reports.FirstOrDefault();
        }

        private void InitializeFiles() {
            var profilePath = RevitRepository.ProfilePath;
            var path = Path.Combine(profilePath, ModuleEnvironment.RevitVersion, nameof(RevitClashDetective), _revitRepository.GetObjectName());
            if(Directory.Exists(path)) {
                Reports = new ObservableCollection<ReportViewModel>(Directory.GetFiles(path)
                                   .Select(item => new ReportViewModel(_revitRepository, Path.GetFileNameWithoutExtension(item))));
                SelectedReport = Reports.FirstOrDefault();
            }
        }

        private void OpenClashDetector(object p) {
            void action() {
                var command = new DetectiveClashesCommand();
                command.ExecuteCommand(_revitRepository.UiApplication);
            }
            _revitRepository.DoAction(action);
        }


        private void Load(object p) {
            var openWindow = GetPlatformService<IOpenFileDialogService>();
            openWindow.Filter = "AutodeskClashReport (*.html)|*.html|PluginClashReport (*.json)|*.json";

            if(!openWindow.ShowDialog(_revitRepository.GetFileDialogPath())) {
                throw new OperationCanceledException();
            }

            InitializeClashes(openWindow.File.FullName);
            _revitRepository.CommonConfig.LastRunPath = openWindow.File.DirectoryName;
            _revitRepository.CommonConfig.SaveProjectConfig();
        }

        private void InitializeClashes(string path) {
            var name = Path.GetFileNameWithoutExtension(path);
            var clashes = ReportLoader.GetClashes(_revitRepository, path)
                                      ?.ToList();
            var report = new ReportViewModel(_revitRepository, name, clashes);

            Reports = new ObservableCollection<ReportViewModel>(new NameResolver<ReportViewModel>(Reports, new[] { report }).GetCollection());
            SelectedReport = report;
        }

        private void Delete(object p) {
            var mb = GetPlatformService<IMessageBoxService>();
            if(mb.Show("Вы уверены, что хотите удалить файл?", "BIM", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Cancel) == MessageBoxResult.Yes) {
                DeleteConfig(SelectedReport.GetUpdatedConfig());
                Reports.Remove(SelectedReport);
                SelectedReport = Reports.FirstOrDefault();
            }
        }

        private void DeleteConfig(ClashesConfig config) {
            if(File.Exists(config.ProjectConfigPath) && config.ProjectConfigPath.EndsWith(".json", StringComparison.CurrentCultureIgnoreCase)) {
                File.Delete(config.ProjectConfigPath);
            }
        }

        private bool CanDelete(object p) => SelectedReport != null;
    }
}