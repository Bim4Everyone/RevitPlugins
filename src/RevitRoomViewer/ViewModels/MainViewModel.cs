using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomViewer.Models;

namespace RevitRoomViewer.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;

        private string _descriptionText;
        private string _roomDescription;
        private bool _needMeasuring;
        private string _revitVersion;

        public Dictionary<string, RoomSettings> RoomsWithSettings { get; set; } = new Dictionary<string, RoomSettings>();

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;

#if REVIT_2023_OR_LESS
            RevitVersion = "Revit 2023";
#else
            RevitVersion = "Revit 2024";
#endif


            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }


        public string RevitVersion {
            get => _revitVersion;
            set => this.RaiseAndSetIfChanged(ref _revitVersion, value);
        }
        public string DescriptionText {
            get => _descriptionText;
            set => this.RaiseAndSetIfChanged(ref _descriptionText, value);
        }


        public string RoomDescription {
            get => _roomDescription;
            set => this.RaiseAndSetIfChanged(ref _roomDescription, value);
        }

        public bool NeedMeasuring {
            get => _needMeasuring;
            set => this.RaiseAndSetIfChanged(ref _needMeasuring, value);
        }

        private List<RoomSettings> _allRooms;
        public List<RoomSettings> AllRooms {
            get => _allRooms;
            set => this.RaiseAndSetIfChanged(ref _allRooms, value);
        }


        private void LoadView() {

            LoadConfig();
            LoadRooms();
        }

        private void AcceptView() {
            UpdateRoomSettings();
            SaveConfig();
        }

        private void LoadRooms() {

            AllRooms = _revitRepository.GetAllRoomName()
                                       .Select(e => new RoomSettings {
                                           RoomName = e.Name,
                                           Description = RoomsWithSettings.ContainsKey(e.Name)
                                                        ? RoomsWithSettings[e.Name].Description
                                                        : "",
                                           NeedMeasuring = RoomsWithSettings.ContainsKey(e.Name)
                                                        ? RoomsWithSettings[e.Name].NeedMeasuring
                                                        : false
                                       }).ToList();
        }

        private void UpdateRoomSettings() {
            foreach(var room in AllRooms) {
                if(RoomsWithSettings.ContainsKey(room.RoomName)) {
                    RoomsWithSettings[room.RoomName].Description = room.Description;
                    RoomsWithSettings[room.RoomName].NeedMeasuring = room.NeedMeasuring;
                } else {
                    RoomsWithSettings[room.RoomName] = new RoomSettings {
                        RoomName = room.RoomName,
                        Description = room.Description,
                        NeedMeasuring = room.NeedMeasuring
                    };
                }
            }
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

            if(setting != null && setting.RoomsWithSettings != null) {
                RoomsWithSettings = setting.RoomsWithSettings;
            } else {
                RoomsWithSettings = new Dictionary<string, RoomSettings>();
            }
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.RoomsWithSettings = RoomsWithSettings;
            _pluginConfig.SaveProjectConfig();
        }
    }
}

