using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomViewer.Models;

namespace RevitRoomViewer.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _revitVersion;
        private string _errorText;

        private List<LevelViewModel> _levels;
        private LevelViewModel _selectedLevel;
        public Dictionary<string, RoomElement> RoomsWithSettings { get; set; } = new Dictionary<string, RoomElement>();

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

#if REVIT_2023_OR_LESS
            RevitVersion = "Revit 2023 или меньше :) ";
#else
            RevitVersion = "Revit 2024";
#endif


            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
        public string RevitVersion {
            get => _revitVersion;
            set => this.RaiseAndSetIfChanged(ref _revitVersion, value);
        }
        public List<LevelViewModel> Levels {
            get => _levels;
            set => this.RaiseAndSetIfChanged(ref _levels, value);
        }
        public LevelViewModel SelectedLevel {
            get => _selectedLevel;
            set => this.RaiseAndSetIfChanged(ref _selectedLevel, value);
        }

        private void LoadView() {
            LoadConfig();
            Levels = _revitRepository.GetLevelsWithRooms(RoomsWithSettings);
            SelectedLevel = Levels.FirstOrDefault();
        }

        private void AcceptView() {
            foreach(var level in Levels) {
                UpdateRoomSettings(level);
            }
            SaveConfig();
            MessageBox.Show("Данные успешно сохранены", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateRoomSettings(LevelViewModel levelViewModel) {
            var rooms = levelViewModel.Rooms;
            foreach(var room in rooms) {
                RoomsWithSettings[room.Id.ToString()] = room;
            }
        }

        private bool CanAcceptView() {

            if(SelectedLevel != null) {

                if(SelectedLevel.Rooms.Count == 0) {
                    ErrorText = "На уровне нет комнат";
                    return false;
                }

                bool allRoomsWithoutDescription = SelectedLevel.Rooms.All(room => string.IsNullOrEmpty(room.Description));

                if(allRoomsWithoutDescription) {
                    ErrorText = "Ни у одной комнаты нет описания";
                    return false;
                }


            }

            ErrorText = null;
            return true;

        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            if(setting != null && setting.RoomsWithSettings != null)
                RoomsWithSettings = setting.RoomsWithSettings;
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.RoomsWithSettings = RoomsWithSettings;
            _pluginConfig.SaveProjectConfig();
        }
    }
}

