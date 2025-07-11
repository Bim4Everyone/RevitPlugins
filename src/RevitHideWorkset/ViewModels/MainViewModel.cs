using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitHideWorkset.Models;

namespace RevitHideWorkset.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IMessageBoxService _messageBoxService;

    private bool _hasChanges;

    private string _errorText;
    private string _searchName;

    private List<LinkedFileElement> _allLinkedFiles;
    private List<LinkedFileElement> _filteredLinkedFiles;
    private List<WorksetElement> _selectedWorksets;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    public MainViewModel(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;

        AllLinkedFiles = _revitRepository.GetLinkedFiles();

        ApplyFilter();

        SelectedWorksets = [];

        ApplyFilterCommand = RelayCommand.Create(ApplyFilter);
        HideSelectedCommand = RelayCommand.Create(HideSelectedWorksets, HaveSelected);
        ShowSelectedCommand = RelayCommand.Create(ShowSelectedWorksets, HaveSelected);
        ToggleVisibilityCommand = RelayCommand.Create<WorksetElement>(ToggleWorksetVisibility);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    /// <summary>
    /// Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    /// <remarks>В случаях, когда используется немодальное окно, требуется данную команду удалять.</remarks>
    public ICommand AcceptViewCommand { get; }

    public ICommand ToggleVisibilityCommand { get; }

    public ICommand HideSelectedCommand { get; }

    public ICommand ShowSelectedCommand { get; }

    public ICommand ApplyFilterCommand { get; }

    public bool HasChanges {
        get => _hasChanges;
        set => RaiseAndSetIfChanged(ref _hasChanges, value);
    }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string SearchName {
        get => _searchName;
        set => RaiseAndSetIfChanged(ref _searchName, value);
    }

    public List<LinkedFileElement> AllLinkedFiles {
        get => _allLinkedFiles;
        set => RaiseAndSetIfChanged(ref _allLinkedFiles, value);
    }

    public List<LinkedFileElement> FilteredLinkedFiles {
        get => _filteredLinkedFiles;
        set => RaiseAndSetIfChanged(ref _filteredLinkedFiles, value);
    }

    public List<WorksetElement> SelectedWorksets {
        get => _selectedWorksets;
        set => RaiseAndSetIfChanged(ref _selectedWorksets, value);
    }

    private bool HaveSelected() {
        return SelectedWorksets.Count > 0;
    }

    private void ApplyFilter() {
        string search = SearchName?.Trim();

        foreach(var file in AllLinkedFiles) {
            file.FilteredWorksets = string.IsNullOrWhiteSpace(search)
                ? file.AllWorksets.ToList()
                : file.AllWorksets
                    .Where(ws => ws.Name != null &&
                                 ws.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
        }

        FilteredLinkedFiles = AllLinkedFiles
            .Where(file => file.FilteredWorksets.Count > 0)
            .ToList();
    }


    private void ToggleWorksetVisibility(WorksetElement workset) {
        workset.IsOpen = !workset.IsOpen;
        workset.IsChanged = !workset.IsChanged;

        HasChanges = AllLinkedFiles.SelectMany(x => x.AllWorksets).Count(w => w.IsChanged) > 0;
    }

    private void HideSelectedWorksets() {
        foreach(var workset in SelectedWorksets) {
            if(workset.IsOpen) {
                workset.IsOpen = false;
                workset.IsChanged = !workset.IsChanged;
            }
        }

        HasChanges = AllLinkedFiles.SelectMany(x => x.AllWorksets).Count(w => w.IsChanged) > 0;
    }

    private void ShowSelectedWorksets() {
        foreach(var workset in SelectedWorksets) {
            if(!workset.IsOpen) {
                workset.IsOpen = true;
                workset.IsChanged = !workset.IsChanged;
            }
        }

        HasChanges = AllLinkedFiles.SelectMany(x => x.AllWorksets).Count(w => w.IsChanged) > 0;
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        var failedFiles = _revitRepository.ToggleWorksetVisibility(AllLinkedFiles);

        if(failedFiles.Count > 0) {
            string title = _localizationService.GetLocalizedString("GeneralSettings.ErrorMessage");
            string body = _localizationService.GetLocalizedString("GeneralSettings.ErrorConnection");

            string failedList = string.Join(Environment.NewLine, failedFiles);
            _messageBoxService.Show(title, $"{body}:{Environment.NewLine}{failedList}");
        }
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
        if(!HasChanges) {
            string message = _localizationService.GetLocalizedString("MainWindow.NoChanges");

            ErrorText = message;
            return false;
        }

        ErrorText = null;
        return true;
    }
}
