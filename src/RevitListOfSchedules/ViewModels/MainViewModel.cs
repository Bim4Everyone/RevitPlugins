using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _errorText;

    private ObservableCollection<LinkViewModel> _links;
    private ObservableCollection<LinkViewModel> _selectedLinks;

    private ObservableCollection<SheetViewModel> _sheets;
    private ObservableCollection<SheetViewModel> _selectedSheets;


    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        Links = GetLinks();
        Sheets = GetSheets();

        SelectedLinks = new ObservableCollection<LinkViewModel>();
        SelectedSheets = new ObservableCollection<SheetViewModel>();


        foreach(var link in Links) {
            link.SelectionChanged += OnLinkSelectionChanged;
        }
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

    public ObservableCollection<LinkViewModel> Links {
        get => _links;
        set => RaiseAndSetIfChanged(ref _links, value);
    }
    public ObservableCollection<LinkViewModel> SelectedLinks {
        get => _selectedLinks;
        set => RaiseAndSetIfChanged(ref _selectedLinks, value);
    }

    public ObservableCollection<SheetViewModel> Sheets {
        get => _sheets;
        set => RaiseAndSetIfChanged(ref _sheets, value);
    }
    public ObservableCollection<SheetViewModel> SelectedSheets {
        get => _selectedSheets;
        set => RaiseAndSetIfChanged(ref _selectedSheets, value);
    }


    // Загружаем с основного документа все линки через _revitRepository
    private ObservableCollection<LinkViewModel> GetLinks() {
        ObservableCollection<LinkViewModel> links = [];
        foreach(LinkTypeElement linkDocumentType in _revitRepository.GetLinkTypes()) {
            links.Add(new LinkViewModel(linkDocumentType));
        }
        return links;
    }

    // При изменении события, выполняем метод добавления в выбранные линки отмеченный линк
    private void OnLinkSelectionChanged(object sender, EventArgs e) {
        if(sender is LinkViewModel link) {
            if(link.IsChecked) {
                if(!SelectedLinks.Contains(link)) {
                    SelectedLinks.Add(link);
                    AddLinkSheets(link);
                }
            } else {
                if(SelectedLinks.Contains(link)) {
                    SelectedLinks.Remove(link);
                    DeleteLinkSheets(link);
                }
            }
        }
    }

    // Загружаем с основного документа все листы через _revitRepository
    private ObservableCollection<SheetViewModel> GetSheets() {
        return new ObservableCollection<SheetViewModel>(
            _revitRepository.GetSheetElements(_revitRepository.Document)
                .Select(sheetElement => new SheetViewModel(sheetElement))
                .OrderBy(sheetElement => sheetElement.AlbumName)
        );
    }

    private void AddLinkSheets(LinkViewModel linkViewModel) {
        foreach(SheetElement sheetElement in _revitRepository.GetSheetElements(_revitRepository.GetLinkDocument(linkViewModel))) {
            _sheets.Add(new SheetViewModel(sheetElement, linkViewModel.Id));
        }
    }

    private void DeleteLinkSheets(LinkViewModel linkViewModel) {
        for(int i = _sheets.Count - 1; i >= 0; i--) {
            if(_sheets[i].Id == linkViewModel.Id) {
                _sheets.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        LoadConfig();
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        SaveConfig();
    }

    /// <summary>
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    /// <remarks>
    /// В данном методе происходит валидация ввода пользователя и уведомление его о неверных значениях.
    /// В методе проверяемые свойства окна должны быть отсортированы в таком же порядке как в окне (сверху-вниз)
    /// </remarks>
    private bool CanAcceptView() {
        ErrorText = null;
        return false;
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);
        _pluginConfig.SaveProjectConfig();
    }
}
