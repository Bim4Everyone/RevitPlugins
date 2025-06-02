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

    private bool _hasChanges;

    private string _errorText;
    private string _searchName;

    private List<LinkedFileElement> _linkedFiles;
    private List<LinkedFileElement> _filteredLinkedFiles;
    private List<WorksetElement> _selectedWorksets;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    public MainViewModel(
        RevitRepository revitRepository,
        ILocalizationService localizationService) {

        _revitRepository = revitRepository;
        _localizationService = localizationService;


        LinkedFiles = _revitRepository.GetLinkedFiles();
        ApplyFilter();

        SelectedWorksets = [];

        HideSelectedCommand = RelayCommand.Create(HideSelectedWorksets, CanHideSelected);
        ToggleVisibilityCommand = RelayCommand.Create<WorksetElement>(ToggleWorksetVisibility);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        _localizationService = localizationService;
    }

    /// <summary>
    /// Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    /// <remarks>В случаях, когда используется немодальное окно, требуется данную команду удалять.</remarks>
    public ICommand AcceptViewCommand { get; }

    public ICommand ToggleVisibilityCommand { get; }

    public ICommand HideSelectedCommand { get; }

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
        set {
            RaiseAndSetIfChanged(ref _searchName, value);
            ApplyFilter();
        }
    }

    public List<LinkedFileElement> LinkedFiles {
        get => _linkedFiles;
        set => RaiseAndSetIfChanged(ref _linkedFiles, value);
    }

    public List<LinkedFileElement> FilteredLinkedFiles {
        get => _filteredLinkedFiles;
        set => RaiseAndSetIfChanged(ref _filteredLinkedFiles, value);
    }

    public List<WorksetElement> SelectedWorksets {
        get => _selectedWorksets;
        set => RaiseAndSetIfChanged(ref _selectedWorksets, value);
    }

    private bool CanHideSelected() {
        return SelectedWorksets.Count > 0;
    }

    private void ApplyFilter() {
        if(string.IsNullOrWhiteSpace(SearchName)) {
            FilteredLinkedFiles = LinkedFiles
                .Select(file => new LinkedFileElement {
                    LinkedFile = file.LinkedFile,
                    Worksets = file.Worksets.ToList()
                })
                .ToList();
        } else {
            string search = SearchName.Trim();

            FilteredLinkedFiles = LinkedFiles
                .Select(file => new LinkedFileElement {
                    LinkedFile = file.LinkedFile,
                    Worksets = file.Worksets
                        .Where(ws => ws.Name != null &&
                                     ws.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList()
                })
                .Where(file => file.Worksets.Count > 0)
                .ToList();
        }
    }

    private void ToggleWorksetVisibility(WorksetElement workset) {
        workset.IsOpen = !workset.IsOpen;
        workset.IsChanged = !workset.IsChanged;

        HasChanges = LinkedFiles.SelectMany(x => x.Worksets).Count(w => w.IsChanged) > 0;
    }

    private void HideSelectedWorksets() {
        foreach(var workset in SelectedWorksets) {
            if(workset.IsOpen) {
                workset.IsOpen = false;
                workset.IsChanged = !workset.IsChanged;
            }
        }

        HasChanges = LinkedFiles.SelectMany(x => x.Worksets).Count(w => w.IsChanged) > 0;
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        _revitRepository.ToggleWorksetVisibility(LinkedFiles);
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
