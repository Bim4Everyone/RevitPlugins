using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.RevitClashReport;

namespace RevitClashDetective.ViewModels.Navigator {
    internal class RevitReportClashesViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _name;
        private ObservableCollection<ClashViewModel> _clashes;

        public RevitReportClashesViewModel(RevitRepository revitRepository, string path = null) {
            _revitRepository = revitRepository;

            if(!string.IsNullOrEmpty(path)) {
                InitializeClashes(path);
            }

            LoadReportCommand = new RelayCommand(LoadReport);
            SelectClashCommand = new RelayCommand(SelectClash, CanSelectClash);
        }

        public ICommand LoadReportCommand { get; }
        public ICommand SelectClashCommand { get; }
        public ObservableCollection<ClashViewModel> Clashes {
            get => _clashes;
            set => this.RaiseAndSetIfChanged(ref _clashes, value);
        }
        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private void LoadReport(object p) {
            var openWindow = GetPlatformService<IOpenFileDialogService>();
            openWindow.Filter = "ClashReport |*.html";
            if(!openWindow.ShowDialog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop))) {
                throw new OperationCanceledException();
            }

            InitializeClashes(openWindow.File.FullName);
        }

        private void InitializeClashes(string path) {
            Name = Path.GetFileNameWithoutExtension(path);
            ReportLoader reportLoader = new ReportLoader(_revitRepository);
            Clashes = new ObservableCollection<ClashViewModel>(reportLoader.GetClashes(path)
                                    .Select(item => new ClashViewModel(_revitRepository, item)));
        }

        private void SelectClash(object p) {
            var clash = (ClashViewModel) p;
            var elements = new[] {_revitRepository.GetElement(clash.Clash.MainElement.DocumentName, clash.Clash.MainElement.Id),
                                  _revitRepository.GetElement(clash.Clash.OtherElement.DocumentName, clash.Clash.OtherElement.Id)};
            _revitRepository.SelectAndShowElement(elements.Where(item => item != null));
        }

        private bool CanSelectClash(object p) {
            return p != null && p is ClashViewModel;
        }
    }
}
