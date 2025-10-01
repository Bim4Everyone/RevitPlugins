using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.RevitClashReport;

namespace RevitClashDetective.ViewModels.Navigator;
internal class RevitReportClashesViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private string _name;
    private ObservableCollection<ClashViewModel> _clashes;

    public RevitReportClashesViewModel(RevitRepository revitRepository, string path = null) {
        _revitRepository = revitRepository;

        if(!string.IsNullOrEmpty(path)) {
            InitializeClashes(path);
        }

        LoadReportCommand = RelayCommand.Create(LoadReport);
        SelectClashCommand = RelayCommand.Create<ClashViewModel>(SelectClash, CanSelectClash);
    }

    public ICommand LoadReportCommand { get; }
    public ICommand SelectClashCommand { get; }
    public ObservableCollection<ClashViewModel> Clashes {
        get => _clashes;
        set => RaiseAndSetIfChanged(ref _clashes, value);
    }
    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    private void LoadReport() {
        var openWindow = GetPlatformService<IOpenFileDialogService>();
        openWindow.Filter = "ClashReport |*.xml";

        if(!openWindow.ShowDialog(_revitRepository.GetFileDialogPath())) {
            throw new OperationCanceledException();
        }

        InitializeClashes(openWindow.File.FullName);
        _revitRepository.CommonConfig.LastRunPath = openWindow.File.DirectoryName;
        _revitRepository.CommonConfig.SaveProjectConfig();
    }

    private void InitializeClashes(string path) {
        Name = Path.GetFileNameWithoutExtension(path);
        var reportLoader = new RevitClashesLoader(_revitRepository, path);
        if(reportLoader.IsValid()) {
            var report = reportLoader.GetReports().First();
            Name = report.Name;
            Clashes = new ObservableCollection<ClashViewModel>(report.Clashes
                    .Select(item => new ClashViewModel(_revitRepository, item)));
        }
    }

    private void SelectClash(ClashViewModel clash) {
        var elements = new[] {
            clash.Clash.MainElement,
            clash.Clash.OtherElement
        };
        _revitRepository.SelectAndShowElement(elements.Where(item => item != null));
    }

    private bool CanSelectClash(ClashViewModel p) {
        return p != null;
    }
}
