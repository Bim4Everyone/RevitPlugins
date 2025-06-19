using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRemoveRoomTags.Models;
using RevitRemoveRoomTags.Views;

namespace RevitRemoveRoomTags.ViewModels;
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;

    private ObservableCollection<View> _selectedViews = [];
    private ObservableCollection<RoomTagTaskHelper> _roomTagTasks = [new RoomTagTaskHelper()];
    private RoomTagTaskHelper _selectedRoomTagTask;
    private bool _needOpenSelectedViews = false;

    private string _errorText;
    private string _errorTextFromGUI;


    public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        AddTaskCommand = RelayCommand.Create(AddTask);
        DeleteTaskCommand = RelayCommand.Create(DeleteTask, CanDeleteTask);

        SelectRoomTagsCommand = new RelayCommand(SelectRoomTags);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public ICommand AddTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }
    public ICommand SelectRoomTagsCommand { get; }


    public ObservableCollection<View> SelectedViews {
        get => _selectedViews;
        set => RaiseAndSetIfChanged(ref _selectedViews, value);
    }

    public ObservableCollection<RoomTagTaskHelper> RoomTagTasks {
        get => _roomTagTasks;
        set => RaiseAndSetIfChanged(ref _roomTagTasks, value);
    }

    public RoomTagTaskHelper SelectedRoomTagTask {
        get => _selectedRoomTagTask;
        set => RaiseAndSetIfChanged(ref _selectedRoomTagTask, value);
    }

    public bool NeedOpenSelectedViews {
        get => _needOpenSelectedViews;
        set => RaiseAndSetIfChanged(ref _needOpenSelectedViews, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string ErrorTextFromGUI {
        get => _errorTextFromGUI;
        set => RaiseAndSetIfChanged(ref _errorTextFromGUI, value);
    }



    /// <summary>
    /// Метод, отрабатывающий при загрузке окна
    /// </summary>
    private void LoadView() {

        LoadConfig();
        GetSelectedViews();
    }

    /// <summary>
    /// Метод, отрабатывающий при нажатии кнопки "Ок"
    /// </summary>
    private void AcceptView() {

        SaveConfig();
        MoveOrRemoveRoomTags();
    }

    /// <summary>
    /// Определяет можно ли запустить работу плагина
    /// </summary>
    private bool CanAcceptView() {

        if(SelectedViews.Count == 0) {
            ErrorText = "Не выбрано ни одного вида";
            return false;
        }

        if(RoomTagTasks.Count == 0) {
            ErrorText = "Не создано ни одной задачи";
            return false;
        }

        foreach(var roomTagTask in RoomTagTasks) {

            if(roomTagTask.RoomTags.Count == 0) {
                ErrorText = "Не во всех задачах выбраны марки помещений";
                return false;
            }
        }

        ErrorText = ErrorTextFromGUI == string.Empty ? string.Empty : ErrorTextFromGUI;

        return true;
    }


    /// <summary>
    /// Подгружает параметры плагина с предыдущего запуска
    /// </summary>
    private void LoadConfig() {

        var settings = _pluginConfig.GetSettings(_revitRepository.Document);

        if(settings is null) { return; }

        NeedOpenSelectedViews = settings.NeedOpenSelectedViews;
    }


    /// <summary>
    /// Сохраняет параметры плагина для следующего запуска
    /// </summary>
    private void SaveConfig() {

        var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);

        settings.NeedOpenSelectedViews = NeedOpenSelectedViews;

        _pluginConfig.SaveProjectConfig();
    }

    /// <summary>
    /// Получает виды выбранные пользователем до запуска плагина
    /// </summary>
    private void GetSelectedViews() {

        foreach(var id in _revitRepository.ActiveUIDocument.Selection.GetElementIds()) {
            if(_revitRepository.Document.GetElement(id) is View view) {
                SelectedViews.Add(view);
            }
        }
    }

    /// <summary>
    /// Метод команды по выбору марок помещений для конкретной задачи RoomTagTaskHelper, которая передается через CommandParameter
    /// </summary>
    private void SelectRoomTags(object obj) {
        if(obj is RoomTagTaskHelper task) {
            task.RoomTags.Clear();

            ISelectionFilter selectFilter = new RoomTagSelectionFilter();
            var references = _revitRepository.ActiveUIDocument.Selection
                            .PickObjects(ObjectType.Element, selectFilter, "Выберите марки помещений на виде");

            foreach(var reference in references) {
                if(_revitRepository.Document.GetElement(reference) is not RoomTag elem) {
                    continue;
                }

                task.RoomTags.Add(elem);
            }
        }

        // Переоткрываем окно плагина
        var mainWindow = new MainWindow {
            DataContext = this
        };
        _ = mainWindow.ShowDialog();
    }


    /// <summary>
    /// Метод, который открывает выбранные виды в сеансе Revit
    /// </summary>
    private void OpenSelectedViews() {

        var activeView = _revitRepository.ActiveUIDocument.ActiveView;

        foreach(var view in SelectedViews) {

            _revitRepository.ActiveUIDocument.ActiveView = view;
        }

        _revitRepository.ActiveUIDocument.ActiveView = activeView;
    }


    /// <summary>
    /// Метод, который отрабатывает при нажатии кнопки "Ок". 
    /// Выполняет перенос марок помещений или их удаление в зависимости от конфигурации задач, созданных пользователем
    /// </summary>
    private void MoveOrRemoveRoomTags() {

        if(NeedOpenSelectedViews) {

            OpenSelectedViews();
        }

        using var transaction = _revitRepository.Document.StartTransaction("Работа с марками помещений");

        HashSet<ElementId> tagsForDel = [];

        // Перебираем задачи, созданные пользователем
        foreach(RoomTagTaskHelper roomTagTask in RoomTagTasks) {

            // Получаем значения смещения, если пользователь задал перемещение марок, а не удаление
            double xOffset;
            double yOffset;
            if(!roomTagTask.RemoveTags) {
                xOffset = UnitUtilsHelper.ConvertToInternalValue(roomTagTask.XOffset);
                yOffset = UnitUtilsHelper.ConvertToInternalValue(roomTagTask.YOffset);
            } else {
                xOffset = 0;
                yOffset = 0;
            }

            // Перебираем выбранные пользователем марки помещений в рамках одной задачи
            foreach(var roomTag in roomTagTask.RoomTags) {

                // Получаем точку положения текстового блока марки
                var tagHeaderPointForCheck = roomTag.TagHeadPosition;

                // Перебираем виды, выбранные пользователем перед запуском плагина
                foreach(var view in SelectedViews) {

                    var roomTagsOnAnotherViews = new FilteredElementCollector(_revitRepository.Document, view.Id)
                        .OfClass(typeof(SpatialElementTag))
                        .OfType<SpatialElementTag>()
                        .ToList();

                    // Перебираем найденные на виде марки помещений
                    foreach(var tag in roomTagsOnAnotherViews) {

                        // Получаем точку текстового блока некой марки,если она совпадает с ранее запрошенной, то работаем с ней
                        var tagHeaderPointCurrent = tag.TagHeadPosition;
                        if(new UV(tagHeaderPointCurrent.X, tagHeaderPointCurrent.Y).IsAlmostEqualTo(new UV(tagHeaderPointForCheck.X, tagHeaderPointForCheck.Y))) {

                            // Если пользователь выбрал в задаче удаление марок, то сохраняем найденные марки с той же позицией и затем удалим
                            if(roomTagTask.RemoveTags) {
                                tagsForDel.Add(tag.Id);
                            } else {
                                tag.HasLeader = true;
                                double z = tag.LeaderEnd.Z;
                                tag.LeaderEnd = new XYZ(
                                    tagHeaderPointCurrent.X,
                                    tagHeaderPointCurrent.Y,
                                    z);

                                tag.TagHeadPosition = new XYZ(
                                    tagHeaderPointCurrent.X + xOffset,
                                    tagHeaderPointCurrent.Y + yOffset,
                                    tagHeaderPointCurrent.Z);
                            }
                        }
                    }
                }
            }
        }
        // Если есть марки, которые нужно удалить - удаляем
        if(tagsForDel.Count > 0) {
            _revitRepository.Document.Delete(tagsForDel);
        }

        transaction.Commit();
    }


    /// <summary>
    /// Метод команды по добавлению новой пустой задачи в список задач
    /// </summary>
    private void AddTask() {

        RoomTagTasks.Add(new RoomTagTaskHelper());
    }

    /// <summary>
    /// Метод команды по удалению выбранной задачи из списка задач
    /// </summary>
    private void DeleteTask() {

        if(SelectedRoomTagTask != null) {
            RoomTagTasks.Remove(SelectedRoomTagTask);
        }
    }

    /// <summary>
    /// Дате возможность удалить задачу из списка задач, в случае если в интерфейсе выбрана хоть одна задача
    /// </summary>
    /// <returns></returns>
    private bool CanDeleteTask() {

        return SelectedRoomTagTask != null;
    }
}
