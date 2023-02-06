using System;
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
using RevitCheckingLevels.Services;
using RevitCheckingLevels.Views;

namespace RevitCheckingLevels.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private LevelViewModel _level;
        private LinkTypeViewModel _linkType
            ;

        public MainViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;

            ViewLoadCommand = new RelayCommand(LoadView);
            UpdateElevationCommand = new RelayCommand(UpdateElevation, CanUpdateElevation);
        }

        public ICommand ViewLoadCommand { get; }
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

        public ObservableCollection<LinkTypeViewModel> LinkTypes { get; } = new ObservableCollection<LinkTypeViewModel>();

        public LinkTypeViewModel LinkType {
            get => _linkType;
            set => this.RaiseAndSetIfChanged(ref _linkType, value);
        }

        private void LoadView(object p) {
            LoadLinkFiles();
            LoadLevelErrors();
        }

        private void LoadLinkFiles() {
            LinkTypes.Clear();
            foreach(RevitLinkType linkType in _revitRepository.GetRevitLinkTypes()) {
                LinkTypes.Add(new LinkTypeViewModel(linkType));
            }

            LinkType = LinkTypes.FirstOrDefault();
        }

        private void LoadLevelErrors() {
            Levels.Clear();
            var levelInfos = _revitRepository.GetLevels()
                .Select(item => new LevelParserImpl(item).ReadLevelInfo())
                .OrderBy(item => item.Level.Elevation)
                .ToArray();

            foreach(LevelInfo levelInfo in levelInfos) {
                if(levelInfo.IsNotStandard()) {
                    Levels.Add(new LevelViewModel(levelInfo) { ErrorType = ErrorType.NotStandard });
                }

                if(levelInfo.IsNotElevation()) {
                    Levels.Add(new LevelViewModel(levelInfo) { ErrorType = ErrorType.NotElevation });
                }

                if(levelInfo.IsNotMillimeterElevation()) {
                    Levels.Add(new LevelViewModel(levelInfo) { ErrorType = ErrorType.NotMillimeterElevation });
                }

                if(levelInfo.IsNotRangeElevation(levelInfos)) {
                    Levels.Add(new LevelViewModel(levelInfo) { ErrorType = ErrorType.NotRangeElevation });
                }
            }

            LoadLinkLevels(levelInfos);
            Level = Levels.FirstOrDefault();
        }

        private void LoadLinkLevels(LevelInfo[] levelInfos) {
            if(LinkType?.IsLinkLoaded == true) {
                var linkLevelInfos = _revitRepository.GetLevels(LinkType.Element)
                    .Select(item => new LevelParserImpl(item).ReadLevelInfo())
                    .OrderBy(item => item.Level.Elevation)
                    .ToArray();

                foreach(LevelInfo levelInfo in levelInfos) {
                    if(levelInfo.IsNotFoundLevels(linkLevelInfos)) {
                        Levels.Add(new LevelViewModel(levelInfo) { ErrorType = ErrorType.NotFoundLevels });
                    }
                }

                foreach(LevelInfo linkLevelInfo in linkLevelInfos) {
                    if(linkLevelInfo.IsNotFoundLinkLevels(levelInfos)) {
                        Levels.Add(new LevelViewModel(linkLevelInfo) { ErrorType = ErrorType.NotFoundLinkLevels });
                    }
                }
            }
        }

        private void UpdateElevation(object p) {
            if(p is object[] list) {
                _revitRepository.UpdateElevations(list
                    .OfType<LevelViewModel>()
                    .Select(item => item.LevelInfo));

                LoadView(null);
            }
        }

        private bool CanUpdateElevation(object p) {
            if(p is object[] list) {
                return list.OfType<LevelViewModel>()
                    .All(item => item.ErrorType == ErrorType.NotElevation);
            }

            return false;
        }
    }
}