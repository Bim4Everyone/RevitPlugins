using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitListOfSchedules.Comparators;
using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels;
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly FamilyLoadOptions _familyLoadOptions;
    private readonly ParamFactory _paramFactory;
    private string _errorText;
    private string _errorLinkText;
    private ObservableCollection<LinkViewModel> _links;
    private ObservableCollection<LinkViewModel> _selectedLinks;
    private ObservableCollection<SheetViewModel> _sheets;
    private ObservableCollection<SheetViewModel> _selectedSheets;
    private ObservableCollection<GroupParameterViewModel> _groupParameters;
    private GroupParameterViewModel _selectedGroupParameter;
    private bool _isCreateScheduleChecked;
    private bool _isScheduleToSheetChecked;
    private bool _isRevisionVisibleChecked;

    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        FamilyLoadOptions familyLoadOptions,
        ParamFactory paramFactory,
        IMessageBoxService messageBoxService) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _familyLoadOptions = familyLoadOptions;
        _paramFactory = paramFactory;
        MessageBoxService = messageBoxService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        ReloadLinksCommand = RelayCommand.Create(ReloadLinks, CanReloadLinks);
        UpdateSelectedSheetsCommand = RelayCommand.Create<IList>(UpdateSelectedSheets);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand ReloadLinksCommand { get; }
    public ICommand UpdateSelectedSheetsCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public IMessageBoxService MessageBoxService { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }
    public string ErrorLinkText {
        get => _errorLinkText;
        set => RaiseAndSetIfChanged(ref _errorLinkText, value);
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
    public bool IsCreateScheduleChecked {
        get => _isCreateScheduleChecked;
        set => RaiseAndSetIfChanged(ref _isCreateScheduleChecked, value);
    }
    public bool IsScheduleToSheetChecked {
        get => _isScheduleToSheetChecked;
        set => RaiseAndSetIfChanged(ref _isScheduleToSheetChecked, value);
    }
    public bool IsRevisionVisibleChecked {
        get => _isRevisionVisibleChecked;
        set => RaiseAndSetIfChanged(ref _isRevisionVisibleChecked, value);
    }

    // Метод загрузки окна
    private void LoadView() {
        Links = new ObservableCollection<LinkViewModel>(GetLinks());
        SelectedLinks = [];
        GroupParameters = new ObservableCollection<GroupParameterViewModel>(GetGroupParameters());
        SelectedGroupParameter = GroupParameters.First();
        Sheets = new ObservableCollection<SheetViewModel>(GetSheets());
        SelectedSheets = [];

        // Подписка на события для обновления Sheets
        PropertyChanged += OnPropertyChanged;

        // Подписка на события в LinkViewModel
        foreach(var link in Links) {
            link.PropertyChanged += OnLinkChanged;
        }
        LoadConfig();
    }

    // Метод загрузки конфигурации пользователя
    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);
        var selectedLinkIds = setting?.SelectedLinks ?? [];
        foreach(var linkId in selectedLinkIds) {
            LinkViewModel link = Links.FirstOrDefault(link => link.Id == linkId);
            if(link != null) {
                link.IsChecked = true;
            }
        }
        string selectedGroupParameterId = setting?.GroupParameter ?? GroupParameters.First().Id;
        SelectedGroupParameter = GroupParameters.First(param => param.Id == selectedGroupParameterId);
        IsCreateScheduleChecked = setting?.IsCreateScheduleChecked ?? false;
        IsRevisionVisibleChecked = setting?.IsRevisionVisibleChecked ?? true;
        IsScheduleToSheetChecked = setting?.IsScheduleToSheetChecked ?? true;
    }

    // Метод подписанный на событие изменения выделенных связанных файлов
    private void OnLinkChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is LinkViewModel link) {
            switch(e.PropertyName) {
                case nameof(LinkViewModel.IsLoaded):
                    AddLinkSheets(link);
                    break;
                case nameof(LinkViewModel.IsChecked):
                    if(!SelectedLinks.Contains(link)) {
                        SelectedLinks.Add(link);
                        if(link.IsLoaded) {
                            AddLinkSheets(link);
                        }
                    } else {
                        if(SelectedLinks.Contains(link)) {
                            DeleteLinkSheets(link);
                            SelectedLinks.Remove(link);
                        }
                    }
                    break;
            }
        }
    }

    // Метод подписанный на событие изменения параметра группировки
    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(SelectedGroupParameter)) {
            UpdateGroupParameter();
        }
    }

    // Метод загрузки с основного документа всех связанных файлов через _revitRepository
    private IEnumerable<LinkViewModel> GetLinks() {
        return _revitRepository.GetLinkTypeElements()
            .Select(item => new LinkViewModel(item, _localizationService))
            .OrderBy(item => item.Name);
    }

    // Метод добавления листов из основного документа
    private IEnumerable<SheetViewModel> GetSheets() {
        return _revitRepository.SheetElements
            .Select(item => new SheetViewModel(_localizationService, item) {
                GroupParameter = SelectedGroupParameter.Parameter
            })
            .OrderBy(item => item, new SheetViewModelComparer());
    }

    // Метод создания SheetViewModel и последующей сортировки
    private IEnumerable<SheetViewModel> GetLinkSheetViewModels(LinkViewModel linkViewModel) {
        var linkDocument = _revitRepository.GetLinkDocument(linkViewModel.Id);
        return _revitRepository.GetSheetElements(linkDocument)
            .Select(item => new SheetViewModel(_localizationService, item, linkViewModel) {
                GroupParameter = SelectedGroupParameter.Parameter
            })
            .OrderBy(item => item, new SheetViewModelComparer());
    }

    // Метод добавления листов из связанных файлов в _sheets.
    private void AddLinkSheets(LinkViewModel linkViewModel) {
        foreach(var sheet in GetLinkSheetViewModels(linkViewModel)) {
            _sheets.Add(sheet);
        }
        SortSheets(_sheets);
    }

    // Метод удаления листов связанных файлов из _sheets
    private void DeleteLinkSheets(LinkViewModel linkViewModel) {
        for(int i = _sheets.Count - 1; i >= 0; i--) {
            if(_sheets[i].LinkViewModel != null && Sheets[i].LinkViewModel.Id == linkViewModel.Id) {
                _sheets.RemoveAt(i);
            }
        }
    }

    // Метод создания GroupParameterViewModel
    private IEnumerable<GroupParameterViewModel> GetGroupParameters() {
        var groupParams = _paramFactory.GetGroupParameters(_revitRepository);
        if(groupParams.Count == 0) {
            yield return new GroupParameterViewModel(_localizationService);
        }
        foreach(var param in groupParams) {
            yield return new GroupParameterViewModel(_localizationService, param);
        }
    }

    // Метод обновления листов в зависимости от параметра
    private void UpdateGroupParameter() {
        foreach(var sheetViewModel in _sheets) {
            sheetViewModel.GroupParameter = SelectedGroupParameter.Parameter;
        }
        SortSheets(_sheets);
    }

    // Метод сортировки листов
    private void SortSheets(ObservableCollection<SheetViewModel> sheetviewmodels) {
        var sorted = sheetviewmodels.OrderBy(item => item, new SheetViewModelComparer()).ToList();
        sheetviewmodels.Clear();
        foreach(var item in sorted) {
            sheetviewmodels.Add(item);
        }
    }

    // Метод обновления списка SelectedSheets в зависимости от выбора пользователя
    private void UpdateSelectedSheets(IList selectedItems) {
        SelectedSheets.Clear();
        foreach(var item in selectedItems.OfType<SheetViewModel>()) {
            SelectedSheets.Add(item);
        }
    }

    // Метод обновления нескольких связанных файлов
    private void ReloadLinks() {
        using(var progressDialogService = ServicesProvider.GetPlatformService<IProgressDialogService>()) {
            progressDialogService.MaxValue = _selectedLinks.Count;
            progressDialogService.StepValue = progressDialogService.MaxValue / 10;
            progressDialogService.DisplayTitleFormat = _localizationService.GetLocalizedString("MainViewModel.ProgressTitleLinks");
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
        SortSheets(_sheets);
        SelectedSheets.Clear();
    }

    // Метод проверки возможности обновления связанных файлов
    private bool CanReloadLinks() {
        if(SelectedLinks == null || SelectedLinks.Count == 0) {
            return false;
        }
        if(SelectedLinks.Any(link => !link.CanReloadLinkType())) {
            ErrorLinkText = _localizationService.GetLocalizedString("MainViewModel.ErrorLinkText");
            return false;
        }
        ErrorLinkText = string.Empty;
        return true;
    }

    // Основной метод
    private void AcceptView() {
        SaveConfig();
        if(_isCreateScheduleChecked) {
            LoadDefaultSchedule();
        }
        SheetProcessing();
    }

    // Проверка возможности выполнения основного метода
    private bool CanAcceptView() {
        if(SelectedSheets != null) {
            if(SelectedSheets.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.ErrorText");
                return false;
            }
        }
        ErrorText = null;
        return true;
    }

    // Метод сохранения конфигурации пользователя
    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
            ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.SelectedLinks = SelectedLinks
            .Select(link => link.Id)
            .ToList();
        setting.GroupParameter = SelectedGroupParameter.Id;
        setting.IsRevisionVisibleChecked = IsRevisionVisibleChecked;
        setting.IsCreateScheduleChecked = IsCreateScheduleChecked;
        setting.IsScheduleToSheetChecked = IsScheduleToSheetChecked;
        _pluginConfig.SaveProjectConfig();
    }

    // Метод обработки листов
    private void SheetProcessing() {
        using var progressService = ServicesProvider.GetPlatformService<IProgressDialogService>();
        progressService.MaxValue = _selectedSheets.Count;
        progressService.StepValue = progressService.MaxValue / 10;
        progressService.DisplayTitleFormat = _localizationService.GetLocalizedString("MainViewModel.ProgressTitleSheets");
        var progress = progressService.CreateProgress();
        var ct = progressService.CreateCancellationToken();
        progressService.Show();

        string transactionName = _localizationService.GetLocalizedString("MainViewModel.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);

        var albums = SelectedSheets.GroupBy(sheet => sheet.AlbumName);
        int counter = 0;
        foreach(var album in albums) {
            string albumName = PathCharValidator.LegalizeString(album.Key);
            var viewDrafting = _revitRepository.GetViewDrafting(albumName);
            var tempDoc = new TempFamilyDocument(_localizationService, _revitRepository, _familyLoadOptions, albumName);
            var famSymbol = tempDoc.GetFamilySymbol();
            var instancesAssembly = new InstancesAssembly(_localizationService, _revitRepository, viewDrafting, famSymbol, albumName);

            instancesAssembly.DeleteFamilyInstances();

            if(_isScheduleToSheetChecked) {
                var firstSheet = album.First();
                instancesAssembly.PlaceFamilyInstance(firstSheet.Number, GetRevisionNumber(firstSheet), ParamFactory.ScheduleName);
            }
            foreach(var sheet in album) {
                ct.ThrowIfCancellationRequested();
                progress.Report(counter++);
                var schedules = _revitRepository.GetScheduleInstances(GetDocument(sheet), sheet.ViewSheet);
                if(schedules != null) {
                    instancesAssembly.PlaceFamilyInstances(sheet.Number, GetRevisionNumber(sheet), schedules);
                }
            }
            if(_isCreateScheduleChecked) {
                _revitRepository.CreateSchedule($"{ParamFactory.DefaultScheduleName}_{albumName}");
                _revitRepository.ActiveUIDocument.GetActiveUIView().Close();
            }
        }
        t.Commit();
    }

    // Метод копирования эталонной спецификации в проект
    private void LoadDefaultSchedule() {
        bool isScheduleChecked = new CheckSchedule(_revitRepository.Application, _revitRepository.ActiveUIDocument)
            .ReplaceSchedule();
        if(!isScheduleChecked) {
            MessageBoxService.Show(
                _localizationService.GetLocalizedString("Common.ScheduleErrorMessageBody"),
                _localizationService.GetLocalizedString("Common.ConfigErrorMessageTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
            throw new OperationCanceledException();
        }
    }

    // Метод получения номера изменения SheetViewModel
    private string GetRevisionNumber(SheetViewModel sheet) {
        return _isRevisionVisibleChecked ? sheet.RevisionNumber : string.Empty;
    }

    // Метод получения номера документа. Либо основной, либо из связанного файла
    private Document GetDocument(SheetViewModel sheet) {
        return sheet.LinkViewModel == null
            ? _revitRepository.Document
            : _revitRepository.GetLinkDocument(sheet.LinkViewModel.Id);
    }
}
