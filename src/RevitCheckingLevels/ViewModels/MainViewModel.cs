using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCheckingLevels.Models;
using RevitCheckingLevels.Models.LevelParser;

namespace RevitCheckingLevels.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly CheckingLevelConfig _checkingLevelConfig;

        private string _errorText;
        private LevelViewModel _level;
        private LinkTypeViewModel _linkType;

        public MainViewModel(RevitRepository revitRepository, CheckingLevelConfig checkingLevelConfig) {
            _revitRepository = revitRepository;
            _checkingLevelConfig = checkingLevelConfig;

            ViewCommand = new RelayCommand(SaveSettings);
            ViewLoadCommand = new RelayCommand(LoadView);
            LoadLevelErrorsCommand = new RelayCommand(LoadLevelErrors);
            UpdateElevationCommand = new RelayCommand(UpdateElevation);
        }

        public ICommand ViewCommand { get; }
        public ICommand ViewLoadCommand { get; }
        public ICommand LoadLevelErrorsCommand { get; }
        public ICommand UpdateElevationCommand { get; }

        public bool IsKoordFile => _revitRepository.IsKoordFile();
        public bool HasErrors => !IsKoordFile && (LinkType == null || !LinkType.IsLinkLoaded) || Levels.Count > 0;

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ObservableCollection<LevelViewModel> Levels { get; } = new ObservableCollection<LevelViewModel>();

        public LevelViewModel Level {
            get => _level;
            set => this.RaiseAndSetIfChanged(ref _level, value);
        }

        public ObservableCollection<LinkTypeViewModel> LinkTypes { get; } =
            new ObservableCollection<LinkTypeViewModel>();

        public LinkTypeViewModel LinkType {
            get => _linkType;
            set => this.RaiseAndSetIfChanged(ref _linkType, value);
        }

        private void SaveSettings(object obj) {
            var settings = _checkingLevelConfig.GetSettings(_revitRepository.Document)
                           ?? _checkingLevelConfig.AddSettings(_revitRepository.Document);

            settings.LinkTypeId = LinkType?.Id ?? ElementId.InvalidElementId;
            _checkingLevelConfig.SaveProjectConfig();
        }

        private void LoadView(object p) {
            LoadLinkFiles(null);
            LoadLevelErrors((object) null);
        }

        private void LoadLinkFiles(object parameter) {
            LinkTypes.Clear();

            if(!IsKoordFile) {
                foreach(RevitLinkType linkType in _revitRepository.GetRevitLinkTypes()) {
                    LinkTypes.Add(new LinkTypeViewModel(linkType));
                }

                // var settings = _checkingLevelConfig.GetSettings(_revitRepository.Document);
                LinkType = LinkTypes.FirstOrDefault(item => _revitRepository.IsKoordFile(item.Element));
            }
        }

        private void LoadLevelErrors(object parameter) {
            Levels.Clear();
            var levelInfos = _revitRepository.GetLevels()
                .Select(item => new LevelParserImpl(item).ReadLevelInfo())
                .OrderBy(item => item.Level.Elevation)
                .ToArray();

            foreach(LevelViewModel item in LoadLevelErrors(levelInfos)) {
                Levels.Add(item);
            }

            LoadLinkLevels(levelInfos);
            Level = Levels.FirstOrDefault();
        }

        private void LoadLinkLevels(LevelInfo[] levelInfos) {
            ErrorText = null;
            if(IsKoordFile) {
                return;
            }

            if(LinkType == null) {
                ErrorText = $"Проверки на соответствие координационному файлу не выполнены (файл не выбран).";
                return;
            }

            if(!LinkType.IsLinkLoaded) {
                ErrorText = $"Проверки на соответствие координационному файлу не выполнены (файл не загружен).";
                return;
            }


            if(!_revitRepository.HasLinkInstance(LinkType.Element)) {
                ErrorText = $"Проверки на соответствие координационному файлу не выполнены (экземпляры не созданы).";
                return;
            }

            var linkLevelInfos = _revitRepository.GetLevels(LinkType.Element)
                .Select(item => new LevelParserImpl(item).ReadLevelInfo())
                .OrderBy(item => item.Level.Elevation)
                .ToArray();

            if(linkLevelInfos.Length == 0) {
                ErrorText = $"Проверки на соответствие координационному файлу не выполнены (нет уровней).";
                return;
            }

            if(LoadLevelErrors(linkLevelInfos).Any()) {
                ErrorText = $"Проверки на соответствие координационному файлу не выполнены (ошибки в коорд. файле).";
                return;
            }

            foreach(LevelInfo levelInfo in levelInfos) {
                if(levelInfo.IsNotFoundLevels(linkLevelInfos)) {
                    Levels.Add(new LevelViewModel(levelInfo) {ErrorType = ErrorType.NotFoundLevels});
                }
            }
        }

        private static IEnumerable<LevelViewModel> LoadLevelErrors(LevelInfo[] levelInfos) {
            foreach(LevelInfo levelInfo in levelInfos) {
                if(levelInfo.IsNotStandard()) {
                    yield return new LevelViewModel(levelInfo) {
                        ErrorType = ErrorType.NotStandard, ToolTipInfo = levelInfo.GetNotStandardTooltip()
                    };
                }

                if(levelInfo.IsNotElevation()) {
                    yield return new LevelViewModel(levelInfo) {
                        ErrorType = ErrorType.NotElevation, ToolTipInfo = levelInfo.GetNotElevationTooltip()
                    };
                }

                if(levelInfo.IsNotMillimeterElevation()) {
                    yield return new LevelViewModel(levelInfo) {
                        ErrorType = ErrorType.NotMillimeterElevation,
                        ToolTipInfo = levelInfo.GetNotMillimeterElevationTooltip()
                    };
                }

                if(levelInfo.IsNotRangeElevation(levelInfos)) {
                    yield return new LevelViewModel(levelInfo) {ErrorType = ErrorType.NotRangeElevation};
                }
            }
        }

        private void UpdateElevation(object p) {
            var levelCreationNames = _revitRepository.GetLevelCreationNames(Levels
                    .Where(item => item.ErrorType == ErrorType.NotElevation)
                    .Select(item => item.LevelInfo))
                .ToArray();

            var duplicateNames = levelCreationNames
                .Where(item => item.DuplicateName)
                .Select(item => $"{item.LevelInfo.Level.Name} -> {item.LevelName}")
                .OrderBy(item => item)
                .ToArray();

            if(duplicateNames.Length > 0) {
                TaskDialog taskDialog = CreateTaskDialog(duplicateNames);
                if(taskDialog.Show() == TaskDialogResult.CommandLink1) {
                    _revitRepository.UpdateElevations(levelCreationNames);
                } else {
                    throw new OperationCanceledException();
                }
            } else {
                _revitRepository.UpdateElevations(levelCreationNames);
            }

            LoadView(null);
        }

        private static TaskDialog CreateTaskDialog(string[] duplicateNames) {
            var taskDialog = new TaskDialog("Обновление отметки");
            taskDialog.TitleAutoPrefix = false;
            taskDialog.AllowCancellation = true;
            taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
            taskDialog.MainContent = "Имена уровней должны быть уникальными";
            taskDialog.MainInstruction = "Обновление отметки в имени невозможно.";
            taskDialog.ExpandedContent = Environment.NewLine + " - "
                                               + string.Join(Environment.NewLine + " - ", duplicateNames);

            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
                "Игнорировать ошибки",
                "Пропускает уровни с дублирующимися именами.");
            taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
                "Отменить",
                "Отменяет переименование всех уровней.");
            return taskDialog;
        }
    }
}