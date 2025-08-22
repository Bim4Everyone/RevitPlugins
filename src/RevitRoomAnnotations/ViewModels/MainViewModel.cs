using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomAnnotations.Models;
using RevitRoomAnnotations.Services;

namespace RevitRoomAnnotations.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IRoomAnnotationMapService _roomAnnotationMapService;
    private readonly IMessageBoxService _messageBoxService;

    private List<LinkedFileViewModel> _linkedFiles;
    private string _errorText;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IRoomAnnotationMapService roomAnnotationMapService,
        IMessageBoxService messageBoxService) {

        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _roomAnnotationMapService = roomAnnotationMapService;
        _messageBoxService = messageBoxService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        _messageBoxService = messageBoxService;
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

    public List<LinkedFileViewModel> LinkedFiles {
        get => _linkedFiles;
        set => RaiseAndSetIfChanged(ref _linkedFiles, value);
    }

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
        LinkedFiles = _revitRepository
           .GetLinkedFiles()
           .OrderByDescending(x => x.IsLoaded)
           .ToList();
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        var linkInstances = GetSelectedLinkInstances();
        var targetView = _revitRepository.CreateOrGetViewDrafting();

        var annotationsNotOnView = GetAnnotationsNotOnView(targetView).ToList();
        var allRoomElements = _revitRepository.GetRoomsElementsFromLinks(linkInstances);
        var rooms = BuildRoomsWithProgress(allRoomElements);
        var annotations = _revitRepository.GetAnnotations().ToList();

        var maps = BuildAnnotationMaps(rooms, annotations).ToList();

        if(!ShowPreview(maps, annotationsNotOnView)) {
            return;
        }

        ProcessChangesWithProgress(maps, annotationsNotOnView);
    }

    private IEnumerable<RevitLinkInstance> GetSelectedLinkInstances() {
        return (LinkedFiles ?? Enumerable.Empty<LinkedFileViewModel>())
            .Where(x => x.IsSelected && x.IsLoaded)
            .Select(x => x.LinkInstance);
    }

    private IEnumerable<RevitAnnotation> GetAnnotationsNotOnView(View targetView) {
        return _revitRepository.GetAnnotationsNotOnTargetView(_revitRepository.Document, targetView);
    }

    private List<RevitRoom> BuildRoomsWithProgress(List<Element> allRoomElements) {
        var rooms = new List<RevitRoom>(allRoomElements?.Count ?? 0);

        using(var window = GetPlatformService<IProgressDialogService>()) {
            string getRooms = _localizationService.GetLocalizedString("MainWindow.GetRooms");
            window.DisplayTitleFormat = $"{getRooms} [{0}/{1}]";
            window.MaxValue = allRoomElements.Count;
            window.StepValue = 1;
            window.Show();

            var progress = window.CreateProgress();
            var ct = window.CreateCancellationToken();

            int i = 1;
            foreach(var roomElem in allRoomElements) {
                progress.Report(i++);
                ct.ThrowIfCancellationRequested();

                rooms.Add(new RevitRoom(roomElem));
            }
        }

        return rooms;
    }

    private IEnumerable<RoomAnnotationMap> BuildAnnotationMaps(
        IEnumerable<RevitRoom> rooms,
        IEnumerable<RevitAnnotation> annotations) {
        return _roomAnnotationMapService.GetRoomAnnotationMap(rooms, annotations);
    }

    private bool ShowPreview(
        IList<RoomAnnotationMap> maps,
        IList<RevitAnnotation> annotationsNotOnView) {
        int toCreate = maps.Count(x => x.ToCreate);
        int toDelete = maps.Count(x => x.ToDelete);
        int toUpdate = maps.Count(x => !x.ToCreate && !x.ToDelete);
        int toDeleteOtherViews = annotationsNotOnView.Count;

        string createRooms = _localizationService.GetLocalizedString("MainWindow.CreateRooms");
        string deleteRooms = _localizationService.GetLocalizedString("MainWindow.DeleteRooms");
        string updateRooms = _localizationService.GetLocalizedString("MainWindow.UpdateRooms");
        string deleteOtherViews = _localizationService.GetLocalizedString("MainWindow.DeleteOtherViews");
        string titlePreview = _localizationService.GetLocalizedString("MainWindow.TitlePreview");

        var deletedIds = maps.Where(x => x.ToDelete && x.RevitAnnotation != null)
                             .Select(x => x.RevitAnnotation.Id)
                             .ToList();

        string previewMsg =
            $"{createRooms}: {toCreate}\n" +
            $"{deleteRooms}: {toDelete} (ID: {string.Join(", ", deletedIds)})\n" +
            $"{deleteOtherViews}: {toDeleteOtherViews}\n" +
            $"{updateRooms}: {toUpdate}";

        return ShowPreviewDialog(titlePreview, previewMsg);
    }

    private void ProcessChangesWithProgress(
        IList<RoomAnnotationMap> maps,
        IList<RevitAnnotation> annotationsNotOnView) {
        string processRooms = _localizationService.GetLocalizedString("MainWindow.ProcessRooms");

        int total =
            annotationsNotOnView.Count +
            maps.Count;

        using var window = GetPlatformService<IProgressDialogService>();
        window.DisplayTitleFormat = $"{processRooms} [{0}/{1}]";
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
                _revitRepository.CreateAnnotation(map.RevitRoom);
            } else if(map.ToDelete) {
                _revitRepository.DeleteElement(map.RevitAnnotation.Annotation);
            } else {
                _revitRepository.UpdateAnnotation(map.RevitAnnotation.Annotation, map.RevitRoom);
            }
            progress.Report(++done);
        }
    }

    private bool ShowPreviewDialog(string title, string msg) {
        return _messageBoxService.Show(msg, title, MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK;
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
        //if(string.IsNullOrEmpty(SaveProperty)) {
        //    ErrorText = _localizationService.GetLocalizedString("MainWindow.HelloCheck");
        //    return false;
        //}

        ErrorText = null;
        return true;
    }
}
