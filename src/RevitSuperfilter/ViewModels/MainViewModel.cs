using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSuperfilter.Models;
using RevitSuperfilter.Services;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    
    private string _errorText;
    private ISuperfilterService _superfilterService;
    private ObservableCollection<ISuperfilterService> _superfilterServices;
   
    private CategoryViewModel _category;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    public MainViewModel(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IReadOnlyCollection<ISuperfilterService> superfilterServices) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        SelectCommand = RelayCommand.Create(SelectElements);
        ShowCommand = RelayCommand.Create(ShowElements);
        IsolateCommand = RelayCommand.Create(IsolateElements);
        
        SuperfilterServices = new ObservableCollection<ISuperfilterService>(superfilterServices);
    }

    public ICommand SelectCommand { get; }

    public ICommand ShowCommand { get; }

    public ICommand IsolateCommand { get; }

    public CategoryViewModel Category {
        get => _category;
        set => this.RaiseAndSetIfChanged(ref _category, value);
    }

    public ISuperfilterService SuperfilterService {
        get => _superfilterService;
        set => this.RaiseAndSetIfChanged(ref _superfilterService, value);
    }

    public ObservableCollection<ISuperfilterService> SuperfilterServices {
        get => _superfilterServices;
        set => this.RaiseAndSetIfChanged(ref _superfilterServices, value);
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

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        foreach(var superfilterService in SuperfilterServices) {
            superfilterService.Build();
        }

        SuperfilterService =
            SuperfilterServices.FirstOrDefault(item =>
                item.Selection == Selection.SelectedOnViewSelection && item.CategoriesViewModel.Categories.Count > 0)
            ?? SuperfilterServices.FirstOrDefault(item =>
                item.Selection == Selection.DBViewSelection && item.CategoriesViewModel.Categories.Count > 0)
            ?? SuperfilterServices.FirstOrDefault(item =>
                item.Selection == Selection.DBSelection && item.CategoriesViewModel.Categories.Count > 0);
    }

    private void SelectElements() {
        _revitRepository.SelectElements(GetCheckedElementIds());
    }

    private void ShowElements() {
        _revitRepository.ShowElements(GetCheckedElementIds());
    }

    private void IsolateElements() {
        _revitRepository.IsolateElements(GetCheckedElementIds());
    }

    private ICollection<ElementId> GetCheckedElementIds() {
        return SuperfilterService?.CategoriesViewModel.GetCheckedElementIds() ?? [];
    }
}
