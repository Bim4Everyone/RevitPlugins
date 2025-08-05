using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCopyStandarts.Models;
using RevitCopyStandarts.Services;

namespace RevitCopyStandarts.ViewModels;

internal sealed class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly IStandartsService _standarts;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;

    private ObservableCollection<BimPartsViewModel> _bimParts;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        IStandartsService standarts,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IProgressDialogFactory progressDialogFactory) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _standarts = standarts;
        _localizationService = localizationService;
        _notificationService = notificationService;
        ProgressDialogFactory = progressDialogFactory;

        LoadViewCommand = RelayCommand.Create(LoadView);
    }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }

    public IProgressDialogFactory ProgressDialogFactory { get; }

    public ObservableCollection<BimPartsViewModel> BimParts {
        get => _bimParts;
        set => RaiseAndSetIfChanged(ref _bimParts, value);
    }

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а также инициализация полей окна.</remarks>
    private void LoadView() {
        IEnumerable<BimPartsViewModel> bimParts = _standarts.GetStandartsFiles()
            .GroupBy(item => _standarts.GetBimPart(item))
            .Select(item => GetBimPart(item.Key, item))
            .OrderBy(item => item.Name);

        BimParts = new ObservableCollection<BimPartsViewModel>(bimParts);
    }

    private BimPartsViewModel GetBimPart(string partName, IEnumerable<FileInfo> bimFiles) {
        BimFileViewModel[] bimFilesVms = bimFiles
            .Select(item =>
                new BimFileViewModel(
                    _standarts.GetFileName(item),
                    item,
                    _revitRepository,
                    _localizationService,
                    _notificationService,
                    ProgressDialogFactory))
            .ToArray();

        return new BimPartsViewModel(partName) {
            MainBimFiles = [..bimFilesVms],
            FilteredBimFiles = [..bimFilesVms]
        };
    }
}
