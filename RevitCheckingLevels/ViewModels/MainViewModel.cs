﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using DevExpress.Data.Native;

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
            LoadLevelErrorsCommand = new RelayCommand(LoadLevelErrors);
            UpdateElevationCommand = new RelayCommand(UpdateElevation, CanUpdateElevation);
        }

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

        public ObservableCollection<LinkTypeViewModel> LinkTypes { get; } = new ObservableCollection<LinkTypeViewModel>();

        public LinkTypeViewModel LinkType {
            get => _linkType;
            set => this.RaiseAndSetIfChanged(ref _linkType, value);
        }

        private void LoadView(object p) {
            LoadLinkFiles(null);
            LoadLevelErrors(null);
        }

        private void LoadLinkFiles(object parameter) {
            LinkTypes.Clear();
            foreach(RevitLinkType linkType in _revitRepository.GetRevitLinkTypes()) {
                LinkTypes.Add(new LinkTypeViewModel(linkType));
            }

            LinkType = LinkTypes.FirstOrDefault();
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
            if(LinkType?.IsLinkLoaded == true) {
                var linkLevelInfos = _revitRepository.GetLevels(LinkType.Element)
                    .Select(item => new LevelParserImpl(item).ReadLevelInfo())
                    .OrderBy(item => item.Level.Elevation)
                    .ToArray();

                if(LoadLevelErrors(levelInfos).Any()) {
                    ErrorText = $"В координационном файле \"{LinkType.Name}\" найдены ошибки в уровнях.";
                    return;
                }

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

        private static IEnumerable<LevelViewModel> LoadLevelErrors(LevelInfo[] levelInfos) {
            foreach(LevelInfo levelInfo in levelInfos) {
                if(levelInfo.IsNotStandard()) {
                    yield return new LevelViewModel(levelInfo) { ErrorType = ErrorType.NotStandard };
                }

                if(levelInfo.IsNotElevation()) {
                    yield return new LevelViewModel(levelInfo) { ErrorType = ErrorType.NotElevation };
                }

                if(levelInfo.IsNotMillimeterElevation()) {
                    yield return new LevelViewModel(levelInfo) { ErrorType = ErrorType.NotMillimeterElevation };
                }

                if(levelInfo.IsNotRangeElevation(levelInfos)) {
                    yield return new LevelViewModel(levelInfo) { ErrorType = ErrorType.NotRangeElevation };
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