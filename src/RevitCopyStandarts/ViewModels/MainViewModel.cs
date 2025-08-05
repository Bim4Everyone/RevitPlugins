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
    private readonly IProgressDialogFactory _progressDialogFactory;

    private string _errorText;
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
        _progressDialogFactory = progressDialogFactory;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }

    /// <summary>
    /// Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    /// <remarks>В случаях, когда используется немодальное окно, требуется данную команду удалять.</remarks>
    public ICommand AcceptViewCommand { get; }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

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

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а также быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        
    }

    /// <summary>
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    /// <remarks>
    /// В данном методе происходит валидация ввода пользователя и уведомление его о неверных значениях.
    /// В методе проверяемые свойства окна должны быть отсортированы в таком же порядке, как в окне (сверху-вниз)
    /// </remarks>
    private bool CanAcceptView() {
        ErrorText = null;
        return true;
    }

    private BimPartsViewModel GetBimPart(string partName, IEnumerable<FileInfo> bimFiles) {
        return new BimPartsViewModel(partName) {
            BimFiles = [
                ..bimFiles.Select(item =>
                    new BimFileViewModel(
                        _standarts.GetFileName(item),
                        item,
                        _revitRepository,
                        _localizationService,
                        _notificationService,
                        _progressDialogFactory))
            ]
        };
    }
}
