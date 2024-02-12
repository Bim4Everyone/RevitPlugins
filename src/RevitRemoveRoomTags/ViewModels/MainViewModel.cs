using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Text;
using System;

using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRemoveRoomTags.Models;
using RevitRemoveRoomTags.Views;
using System.Windows.Input;
using System.Linq;

namespace RevitRemoveRoomTags.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private ObservableCollection<View> _selectedViews = new ObservableCollection<View>();
        private ObservableCollection<RoomTagTaskHelper> _roomTagTasks = new ObservableCollection<RoomTagTaskHelper>() { new RoomTagTaskHelper() };
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
            set => this.RaiseAndSetIfChanged(ref _selectedViews, value);
        }

        public ObservableCollection<RoomTagTaskHelper> RoomTagTasks {
            get => _roomTagTasks;
            set => this.RaiseAndSetIfChanged(ref _roomTagTasks, value);
        }

        public RoomTagTaskHelper SelectedRoomTagTask {
            get => _selectedRoomTagTask;
            set => this.RaiseAndSetIfChanged(ref _selectedRoomTagTask, value);
        }

        public bool NeedOpenSelectedViews {
            get => _needOpenSelectedViews;
            set => this.RaiseAndSetIfChanged(ref _needOpenSelectedViews, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string ErrorTextFromGUI {
            get => _errorTextFromGUI;
            set => this.RaiseAndSetIfChanged(ref _errorTextFromGUI, value);
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

            foreach(RoomTagTaskHelper roomTagTask in RoomTagTasks) {

                if(roomTagTask.RoomTags.Count == 0) {
                    ErrorText = "Не во всех задачах выбраны марки помещений";
                    return false;
                }
            }

            if(ErrorTextFromGUI == string.Empty) {
                ErrorText = string.Empty;
            } else {
                ErrorText = ErrorTextFromGUI;
            }

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

            foreach(ElementId id in _revitRepository.ActiveUIDocument.Selection.GetElementIds()) {

                View view = _revitRepository.Document.GetElement(id) as View;
                if(view != null) {
                    SelectedViews.Add(view);
                }
            }
        }

        /// <summary>
        /// Метод команды по выбору марок помещений для конкретной задачи RoomTagTaskHelper, которая передается через CommandParameter
        /// </summary>
        private void SelectRoomTags(object obj) {

            RoomTagTaskHelper task = obj as RoomTagTaskHelper;
            if(task != null) {
                task.RoomTags.Clear();

                ISelectionFilter selectFilter = new RoomTagSelectionFilter();
                IList<Reference> references = _revitRepository.ActiveUIDocument.Selection
                                .PickObjects(ObjectType.Element, selectFilter, "Выберите марки помещений на виде");

                foreach(Reference reference in references) {

                    RoomTag elem = _revitRepository.Document.GetElement(reference) as RoomTag;
                    if(elem is null) {
                        continue;
                    }

                    task.RoomTags.Add(elem);
                }
            }

            // Переоткрываем окно плагина
            MainWindow mainWindow = new MainWindow();
            mainWindow.DataContext = this;
            mainWindow.ShowDialog();
        }


        /// <summary>
        /// Метод, который открывает выбранные виды в сеансе Revit
        /// </summary>
        private void OpenSelectedViews() {

            View activeView = _revitRepository.ActiveUIDocument.ActiveView;

            foreach(View view in SelectedViews) {

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

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Работа с марками помещений")) {

                HashSet<ElementId> tagsForDel = new HashSet<ElementId>();

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
                    foreach(RoomTag roomTag in roomTagTask.RoomTags) {

                        // Получаем точку положения текстового блока марки
                        XYZ tagHeaderPointForCheck = roomTag.TagHeadPosition;

                        // Перебираем виды, выбранные пользователем перед запуском плагина
                        foreach(View view in SelectedViews) {

                            List<SpatialElementTag> roomTagsOnAnotherViews = new FilteredElementCollector(_revitRepository.Document, view.Id)
                                .OfClass(typeof(SpatialElementTag))
                                .OfType<SpatialElementTag>()
                                .ToList();

                            // Перебираем найденные на виде марки помещений
                            foreach(SpatialElementTag tag in roomTagsOnAnotherViews) {

                                // Получаем точку текстового блока некой марки,если она совпадает с ранее запрошенной, то работаем с ней
                                XYZ tagHeaderPointCurrent = tag.TagHeadPosition;
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
}
