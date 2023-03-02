using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using DevExpress.Data.Native;
using DevExpress.Mvvm.Native;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCheckingLevels.Models;
using RevitCheckingLevels.Models.LevelParser;
using RevitCheckingLevels.Services;
using RevitCheckingLevels.Views;

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

            settings.LinkTypeId = LinkType?.Id.IntegerValue ?? 0;
            _checkingLevelConfig.SaveProjectConfig();
        }

        private void LoadView(object p) {
            LoadLinkFiles(null);
            LoadLevelErrors((object) null);
        }

        private void LoadLinkFiles(object parameter) {
            LinkTypes.Clear();
            foreach(RevitLinkType linkType in _revitRepository.GetRevitLinkTypes()) {
                LinkTypes.Add(new LinkTypeViewModel(linkType));
            }

            var settings = _checkingLevelConfig.GetSettings(_revitRepository.Document);
            LinkType = LinkTypes.FirstOrDefault(item => item.Id.IntegerValue == settings?.LinkTypeId);
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
            if(LinkType?.IsLinkLoaded == false) {
                ErrorText = $"Загрузите координационный файл \"{LinkType.Name}\".";
                return;
            }

            if(LinkType?.IsLinkLoaded == true) {
                if(!_revitRepository.HasLinkInstance(LinkType.Element)) {
                    ErrorText = $"Не были созданы экземпляры связанного файла \"{LinkType.Name}\".";
                    return;
                }

                var linkLevelInfos = _revitRepository.GetLevels(LinkType.Element)
                    .Select(item => new LevelParserImpl(item).ReadLevelInfo())
                    .OrderBy(item => item.Level.Elevation)
                    .ToArray();

                if(linkLevelInfos.Length == 0) {
                    ErrorText = $"В координационном файле \"{LinkType.Name}\" не были найдены уровни.";
                    return;
                }

                if(LoadLevelErrors(levelInfos).Any()) {
                    ErrorText = $"В координационном файле \"{LinkType.Name}\" найдены ошибки в уровнях.";
                    return;
                }

                foreach(LevelInfo levelInfo in levelInfos) {
                    if(levelInfo.IsNotFoundLevels(linkLevelInfos)) {
                        Levels.Add(new LevelViewModel(levelInfo) {ErrorType = ErrorType.NotFoundLevels});
                    }
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
            _revitRepository.UpdateElevations(Levels
                .Where(item => item.ErrorType == ErrorType.NotElevation)
                .Select(item => item.LevelInfo));

            LoadView(null);
        }
    }
}