using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomExtrusion.Models;

namespace RevitRoomExtrusion.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;         

        private string _errorText;
        private string _saveProperty;
        private string _extrusionHeight;
        private string _extrusionFamilyName;
        private List<Room> _errorRooms;
        private List<Room> _goodRooms;


        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository, 
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            
            _localizationService = localizationService;            

            ExtrusionHeight = "2200";
            ExtrusionFamilyName = "Машино-места";

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

            List<Room> selectedRooms = _revitRepository.GetListRoom();
            RoomContourChecker contourChecker = new RoomContourChecker();

            var errorListRoom = new List<Room>();
            var goodListRoom = new List<Room>();

            foreach(Room room in selectedRooms) {
                if(room.IsNotEnclosed() || room.IsRedundant() || contourChecker.IsIntersectBoundary(room)) {
                    errorListRoom.Add(room);
                } else {
                    goodListRoom.Add(room);
                    GoodRooms = goodListRoom;
                }
            }
            if(errorListRoom.Count > 0) {
                ErrorRooms = errorListRoom;
            }
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }
        public string ExtrusionHeight {
            get => _extrusionHeight;
            set => this.RaiseAndSetIfChanged(ref _extrusionHeight, value);
        }
        public string ExtrusionFamilyName {
            get => _extrusionFamilyName;
            set => this.RaiseAndSetIfChanged(ref _extrusionFamilyName, value);
        }
        public List<Room> ErrorRooms {
            get => _errorRooms;
            set => RaiseAndSetIfChanged(ref _errorRooms, value);
        }
        public List<Room> GoodRooms {
            get => _goodRooms;
            set => RaiseAndSetIfChanged(ref _goodRooms, value);
        }



        private void LoadView() {
            LoadConfig();
        }

        private void AcceptView() {
            SaveConfig();
            View3D view3D = null;
            using(Transaction tr = new Transaction(_revitRepository.Document, "Создание 3D вида")) {
                tr.Start();
                view3D = _revitRepository.GetView3D(_extrusionFamilyName);
                tr.Commit();
            }

            List<RoomElement> roomElements = new List<RoomElement>();
            foreach(Room room in GoodRooms) {
                RoomElement roomElement = new RoomElement(_revitRepository, room, view3D);
                roomElements.Add(roomElement);
            }

            var groupedRooms = roomElements.GroupBy(re => re.LocationSlab);

            foreach(IGrouping<double, RoomElement> groupRooms in groupedRooms) {
                using(Transaction tr = new Transaction(_revitRepository.Document, "Загрузка и вставка семейства")) {
                    tr.Start();
                    double location = groupRooms.Key;
                    double extrusionHeightDouble = Convert.ToDouble(_extrusionHeight);

                    FamilyDocument familyDocument = new FamilyDocument(_revitRepository, extrusionHeightDouble, location, groupRooms.ToList(), _extrusionFamilyName);
                    familyDocument.CreateFamily();

                    string famPath = familyDocument.FamPath;
                    string famName = familyDocument.FamName;

                    FamilySymbol famSymbol = _revitRepository.LoadFamily(famPath, famName);

                    File.Delete(famPath);

                    double locationRoom = groupRooms.FirstOrDefault().LocationRoom;
                    double locationPlace = location - locationRoom;

                    _revitRepository.PlaceFamily(famSymbol, locationPlace);

                    tr.Commit();
                }
            }
        }


        private bool CanAcceptView() {
            if(string.IsNullOrEmpty(ExtrusionHeight)) {

                ErrorText = _localizationService.GetLocalizedString("Задайте высоту выдавливания");
                return false;
            }
            if(!double.TryParse(ExtrusionHeight, out double result)) {
                ErrorText = _localizationService.GetLocalizedString("Высота должна быть числом");
                return false;
            }
            if(string.IsNullOrEmpty(ExtrusionFamilyName)) {
                ErrorText = _localizationService.GetLocalizedString("Задайте имя типа помещений");
                return false;
            }
            ErrorText = null;
            return true;
        }
               

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainWindow.Hello");


            ExtrusionHeight = setting?.ExtrusionHeight ?? "2200";            
            ExtrusionFamilyName = setting?.ExtrusionFamilyName ?? "Машино-места";
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.SaveProperty = SaveProperty;

            setting.ExtrusionHeight = ExtrusionHeight;
            setting.ExtrusionFamilyName = ExtrusionFamilyName;

            _pluginConfig.SaveProjectConfig();
        }
    }
}
