using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using DevExpress.Xpf.Grid;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCheckingLevels.Models;
using RevitCheckingLevels.Models.LevelParser;
using RevitCheckingLevels.Services;
using RevitCheckingLevels.Views;

namespace RevitCheckingLevels.ViewModels {
    internal class CheckingLevelsViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private LevelViewModel _level;

        public CheckingLevelsViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;

            ViewLoadCommand = new RelayCommand(Load);
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

        private void Load(object p) {
            Levels.Clear();
            foreach(Level level in _revitRepository.GetLevels()) {
                var levelViewModel = new LevelViewModel(
                    new LevelParserImpl(level).ReadLevelInfo());
                if(levelViewModel.ErrorType != null) {
                    Levels.Add(levelViewModel);
                }
            }

            Level = Levels.FirstOrDefault();
        }

        private void UpdateElevation(object p) {
            if(p is object[] list) {
                _revitRepository.UpdateElevations(list
                    .OfType<LevelViewModel>()
                    .Select(item => item.LevelInfo));

                Load(null);
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