using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using RevitEditingZones.ViewModels;
using RevitEditingZones.Views;

namespace RevitEditingZones.Services {
    public class LevelsWindowService : ILevelsWindowService {
        private readonly Func<LevelsWindow> _levelWindowFactory;

        public LevelsWindowService(Func<LevelsWindow> levelWindowFactory) {
            _levelWindowFactory = levelWindowFactory;
        }

        public bool? ShowLevels(ObservableCollection<LevelViewModel> levels) {
            var window = _levelWindowFactory();
            window.DataContext = levels;
            return window.ShowDialog();
        }
    }

    public interface ILevelsWindowService {
        bool? ShowLevels(ObservableCollection<LevelViewModel> levels);
    }
}