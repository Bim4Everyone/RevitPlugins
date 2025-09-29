using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomAnnotations.Models;
using RevitRoomAnnotations.Services;


namespace RevitRoomAnnotations.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IRoomAnnotationMapService _roomAnnotationMapService;

    private ObservableCollection<LinkViewModel> _links;
    private ObservableCollection<LinkViewModel> _selectedLinks;
    private string _errorText;

    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IRoomAnnotationMapService roomAnnotationMapService,
        IMessageBoxService messageBoxService) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _roomAnnotationMapService = roomAnnotationMapService;
        MessageBoxService = messageBoxService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public IMessageBoxService MessageBoxService { get; }

    public ObservableCollection<LinkViewModel> Links {
        get => _links;
        set => RaiseAndSetIfChanged(ref _links, value);
    }

    public ObservableCollection<LinkViewModel> SelectedLinks {
        get => _selectedLinks;
        set => RaiseAndSetIfChanged(ref _selectedLinks, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    // Метод загрузки конфигурации окна
    private void LoadView() {
        Links = new ObservableCollection<LinkViewModel>(GetLinks());
        SelectedLinks = [];
        // Подписка на события для обновления Links        
        foreach(var link in Links) {
            link.PropertyChanged += OnLinkChanged;
        }
        LoadConfig();
    }

    // Метод загрузки конфигурации пользователя
    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);
        var selectedLinkNames = setting?.SelectedLinks ?? [];
        foreach(string linkName in selectedLinkNames) {
            LinkViewModel link = Links.FirstOrDefault(link => link.Name == linkName);
            if(link != null) {
                link.IsChecked = true;
            }
        }
    }

    // Метод сохранения конфигурации пользователя
    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
            ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.SelectedLinks = SelectedLinks
            .Select(link => link.Name)
            .ToList();
        _pluginConfig.SaveProjectConfig();
    }

    // Метод подписанный на событие изменения выделенных связанных файлов
    private void OnLinkChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == "IsChecked" && sender is LinkViewModel link) {
            if(link.IsChecked) {
                SelectedLinks.Add(link);
            } else {
                SelectedLinks.Remove(link);
            }
        }
    }

    // Метод загрузки с основного документа всех связанных файлов через _revitRepository
    private IEnumerable<LinkViewModel> GetLinks() {
        return _revitRepository.GetLinkInstanceElements()
            .Select(item => new LinkViewModel(item, _localizationService))
            .OrderBy(item => item.Name);
    }

    // Основной метод
    private void AcceptView() {
        SaveConfig();
        string transactionName = _localizationService.GetLocalizedString("MainViewModel.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);

        var linkInstanceElements = SelectedLinks.Select(link => link.LinkInstanceElement);
        var allRoomElements = _revitRepository.GetRevitRoomsFromLinks(linkInstanceElements);
        var rooms = BuildRoomsWithProgress(allRoomElements);

        var targetView = _revitRepository.GetViewDrafting();
        var annotations = _revitRepository.GetRevitAnnotations(targetView, true);

        var maps = BuildAnnotationMaps(rooms, annotations).ToList();
        var annotationsNotOnView = _revitRepository.GetRevitAnnotations(targetView, false).ToList();

        if(!ShowPreview(maps, annotationsNotOnView)) {
            return;
        }
        ProcessChangesWithProgress(maps, annotationsNotOnView, targetView);

        t.Commit();
    }

    // Проверка возможности выполнения основного метода
    private bool CanAcceptView() {
        if(SelectedLinks != null) {
            if(SelectedLinks.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.ErrorTextNoSelection");
                return false;
            }
            if(SelectedLinks.Select(link => link).Where(link => !link.IsLoaded).Any()) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.ErrorTextWrongSelection");
                return false;
            }
        }
        ErrorText = null;
        return true;
    }

    // Метод получения списка помещений 
    private List<RevitRoom> BuildRoomsWithProgress(List<RevitRoom> allRoomElements) {
        var rooms = new List<RevitRoom>(allRoomElements?.Count ?? 0);

        using(var window = GetPlatformService<IProgressDialogService>()) {
            window.DisplayTitleFormat = _localizationService.GetLocalizedString("MainViewModel.ProcessRooms");
            window.MaxValue = allRoomElements.Count;
            window.StepValue = window.MaxValue / 10;
            window.Show();

            var progress = window.CreateProgress();
            var ct = window.CreateCancellationToken();

            int i = 1;
            foreach(var roomElem in allRoomElements) {
                progress.Report(i++);
                ct.ThrowIfCancellationRequested();

                rooms.Add(roomElem);
            }
        }
        return rooms;
    }

    // Метод получения списка соответствий помещений и аннотаций
    private IEnumerable<RoomAnnotationMap> BuildAnnotationMaps(
        IEnumerable<RevitRoom> rooms,
        IEnumerable<RevitAnnotation> annotations) {
        return _roomAnnotationMapService.GetRoomAnnotationMap(rooms, annotations);
    }

    // Метод показа предварительного расчёта
    private bool ShowPreview(
        IList<RoomAnnotationMap> maps,
        IList<RevitAnnotation> annotationsNotOnView) {
        int toCreate = maps.Count(x => x.ToCreate);
        int toDelete = maps.Count(x => x.ToDelete);
        int toUpdate = maps.Count(x => !x.ToCreate && !x.ToDelete);
        int toDeleteOtherViews = annotationsNotOnView.Count;

        string annToCreate = _localizationService.GetLocalizedString("MainViewModel.Message.AnnotationsToCreate", toCreate);
        string annToDelete = _localizationService.GetLocalizedString("MainViewModel.Message.AnnotationsToDelete", toDelete);
        string annToDeleteOtherViews = _localizationService.GetLocalizedString("MainViewModel.Message.AnnotationsToDeleteOtherViews", toDeleteOtherViews);
        string annToUpdate = _localizationService.GetLocalizedString("MainViewModel.Message.AnnotationsToUpdate", toUpdate);
        string title = _localizationService.GetLocalizedString("MainViewModel.Message.Title");

        var stringBuilder = new StringBuilder();
        string message = stringBuilder.Append($"{annToCreate}\n")
                                      .Append($"{annToDelete}\n")
                                      .Append($"{annToDeleteOtherViews}\n")
                                      .Append($"{annToUpdate}")
                                      .ToString();

        return MessageBoxService.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK;
    }

    // Основной метод копирования, удаления или обновления аннотаций
    private void ProcessChangesWithProgress(
        IList<RoomAnnotationMap> maps,
        IList<RevitAnnotation> annotationsNotOnView,
        View view) {
        string processRooms = _localizationService.GetLocalizedString("MainViewModel.ProcessRooms");
        int total = annotationsNotOnView.Count + maps.Count;
        using var window = GetPlatformService<IProgressDialogService>();
        window.DisplayTitleFormat = processRooms;
        window.MaxValue = total;
        window.StepValue = 1;
        window.Show();

        var progress = window.CreateProgress();
        var ct = window.CreateCancellationToken();
        int done = 0;

        foreach(var ann in annotationsNotOnView) {
            ct.ThrowIfCancellationRequested();
            _revitRepository.DeleteElement(ann.Annotation);
            progress.Report(++done);
        }

        foreach(var map in maps) {
            ct.ThrowIfCancellationRequested();

            if(map.ToCreate) {
                _revitRepository.CreateAnnotation(map.RevitRoom, view);
            } else if(map.ToDelete) {
                _revitRepository.DeleteElement(map.RevitAnnotation.Annotation);
            } else {
                _revitRepository.UpdateAnnotation(map.RevitAnnotation.Annotation, map.RevitRoom);
            }
            progress.Report(++done);
        }
    }
}
