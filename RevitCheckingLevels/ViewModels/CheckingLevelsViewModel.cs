using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

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

            ViewCommand = new RelayCommand(Execute);
            ViewLoadCommand = new RelayCommand(Load);
        }

        public ICommand ViewCommand { get; }
        public ICommand ViewLoadCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ObservableCollection<LevelViewModel> Levels { get; }
            = new ObservableCollection<LevelViewModel>();

        public LevelViewModel Level {
            get => _level;
            set => this.RaiseAndSetIfChanged(ref _level, value);
        }

        private void Execute(object p) {

        }

        private void Load(object p) {
            Levels.Clear();
            foreach(Level level in _revitRepository.GetLevels()) {
                Levels.Add(new LevelViewModel(
                    new LevelParserImpl(level).ReadLevelInfo()));
            }

            Level = Levels.FirstOrDefault();
        }
    }
}
