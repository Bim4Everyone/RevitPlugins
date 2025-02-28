using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels;


internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _errorText;
    private ObservableCollection<LinkViewModel> _links;
    private ObservableCollection<LinkViewModel> _selectedLinks;
    private ObservableCollection<SheetViewModel> _sheets;
    private ObservableCollection<SheetViewModel> _selectedSheets;
    private ObservableCollection<GroupParameterViewModel> _groupParameters;
    private GroupParameterViewModel _selectedGroupParameter;


    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        Links = GetLinks();
        SelectedLinks = new ObservableCollection<LinkViewModel>();

        GroupParameters = GetGroupParameters();
        SelectedGroupParameter = GroupParameters.Last();

        Sheets = GetSheets();
        SelectedSheets = new ObservableCollection<SheetViewModel>();

        // Подписка на события для обновления Sheets
        PropertyChanged += OnPropertyChanged;

        // Подписка на события в LinkViewModel
        foreach(var link in Links) {
            link.SelectionChanged += OnLinkSelectionChanged;
        }

        LoadViewCommand = RelayCommand.Create(LoadView);
        ReloadLinksCommand = RelayCommand.Create(ReloadLinks, CanReloadLinks);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand ReloadLinksCommand { get; }
    public ICommand AcceptViewCommand { get; }

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

    public ObservableCollection<GroupParameterViewModel> GroupParameters {
        get => _groupParameters;
        set => RaiseAndSetIfChanged(ref _groupParameters, value);
    }

    public GroupParameterViewModel SelectedGroupParameter {
        get => _selectedGroupParameter;
        set => RaiseAndSetIfChanged(ref _selectedGroupParameter, value);
    }

    // Загружаем с основного документа все линки через _revitRepository
    private ObservableCollection<LinkViewModel> GetLinks() {
        ObservableCollection<LinkViewModel> links = [];
        foreach(LinkTypeElement linkTypeElement in _revitRepository.GetLinkTypeElements()) {
            links.Add(new LinkViewModel(linkTypeElement));
        }
        return links;
    }

    // При изменении события, выполняем метод добавления/удаления в SelectedSheets отмеченный линк и добавляем/удаляем
    // листы в Sheets
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

    private ObservableCollection<SheetViewModel> GetSheets() {
        var mainDocumentSheets = _revitRepository.GetSheetElements(_revitRepository.Document)
            .Select(sheetElement => new SheetViewModel(sheetElement) {
                SheetParameter = SelectedGroupParameter.Parameter
            })
            .ToList();

        foreach(var linkViewModel in SelectedLinks) {
            Document linkDocument = _revitRepository.GetLinkDocument(linkViewModel);
            var linkDocumentSheets = _revitRepository.GetSheetElements(linkDocument)
                .Select(sheetElement => new SheetViewModel(sheetElement) {
                    SheetParameter = SelectedGroupParameter.Parameter
                });
            mainDocumentSheets.AddRange(linkDocumentSheets);
        }
        return SortSheets(new ObservableCollection<SheetViewModel>(mainDocumentSheets));
    }


    // Добавляем листы из связей в _sheets.
    private void AddLinkSheets(LinkViewModel linkViewModel) {
        foreach(SheetElement sheetElement in _revitRepository.GetSheetElements(_revitRepository.GetLinkDocument(linkViewModel))) {
            _sheets.Add(new SheetViewModel(sheetElement, linkViewModel.Id) {
                SheetParameter = SelectedGroupParameter.Parameter
            });
        }
        UpdateGroupParameter();
    }

    // Удаляем листы из _sheets из связей
    private void DeleteLinkSheets(LinkViewModel linkViewModel) {
        for(int i = _sheets.Count - 1; i >= 0; i--) {
            if(_sheets[i].LinkTypeId == linkViewModel.Id) {
                _sheets.RemoveAt(i);
            }
        }
    }

    // Добавляем в список GroupParameters
    private ObservableCollection<GroupParameterViewModel> GetGroupParameters() {
        var firstSheet = _revitRepository.GetViewSheets(_revitRepository.Document)
            .FirstOrDefault();

        ObservableCollection<GroupParameterViewModel> list = [];
        if(firstSheet != null) {
            IList<Parameter> listOfParameter = _revitRepository.GetBrowserParameters(firstSheet);
            if(listOfParameter != null) {
                foreach(Parameter parameter in listOfParameter) {
                    list.Add(new GroupParameterViewModel(parameter));
                }
            } else {
                list.Add(new GroupParameterViewModel());
            }
        } else {
            list.Add(new GroupParameterViewModel());
        }
        return list;
    }

    // В зависимости от SelectedGroupParameter обновляем сортировку в Sheets
    private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(SelectedGroupParameter)) {
            UpdateGroupParameter();
        }
    }
    // Метод обновления листов в Sheets
    private void UpdateGroupParameter() {
        var newSheets = new ObservableCollection<SheetViewModel>(
            _sheets
            .Select(sheetViewModel => {
                sheetViewModel.SheetParameter = SelectedGroupParameter.Parameter;
                return sheetViewModel;
            }));

        Sheets = SortSheets(newSheets);
    }

    private ObservableCollection<SheetViewModel> SortSheets(ObservableCollection<SheetViewModel> sheetViewModels) {
        var sortedSheets = sheetViewModels
            .OrderBy(sheetViewModel => GetIntFromString(sheetViewModel.Number))
            .OrderBy(sheetViewModel => sheetViewModel.AlbumName)
            .ToList();
        return new ObservableCollection<SheetViewModel>(sortedSheets);
    }

    private double GetIntFromString(string stringParameter) {
        if(int.TryParse(stringParameter, out var value)) {
            return value;
        }
        return 0;
    }

    private void ReloadLinks() {

        using(var progressDialogService = ServicesProvider.GetPlatformService<IProgressDialogService>()) {
            progressDialogService.MaxValue = _selectedLinks.Count;
            progressDialogService.StepValue = progressDialogService.MaxValue / 10;
            progressDialogService.DisplayTitleFormat = "Обновление связанных файлов... [{0}]\\[{1}]";
            var progress = progressDialogService.CreateProgress();
            var ct = progressDialogService.CreateCancellationToken();
            progressDialogService.Show();

            int i = 0;
            foreach(var selectedLink in _selectedLinks) {
                ct.ThrowIfCancellationRequested();
                progress.Report(i++);
                selectedLink.ReloadLinkType();
            }
        }
        Sheets = GetSheets();
    }

    private bool CanReloadLinks() {
        if(_selectedLinks.Count > 0) {
            return true;
        } else {
            return false;
        }
    }



    private void LoadView() {
        LoadConfig();
    }

    private void AcceptView() {
        SaveConfig();
    }

    private bool CanAcceptView() {
        if(SelectedSheets.Count == 0) {
            ErrorText = "Выберите листы для создания ведомости";
            return false;
        }
        ErrorText = null;
        return true;
    }

    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
    }

    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);
        _pluginConfig.SaveProjectConfig();
    }
}

