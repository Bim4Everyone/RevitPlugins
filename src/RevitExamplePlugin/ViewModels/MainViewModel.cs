using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitExamplePlugin.Models;

namespace RevitExamplePlugin.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly WallRevitRepository _wallRevitRepository;
        private readonly ILocalizationService _localizationService;

        private string _errorText;

        private double _height;
        private CustomLocation _customLocation;

        private ObservableCollection<WallViewModel> _walls;

        private WallTypeViewModel _wallType;
        private ObservableCollection<WallTypeViewModel> _wallTypes;

        public MainViewModel(
            PluginConfig pluginConfig,
            WallRevitRepository wallRevitRepository,
            ILocalizationService localizationService) {

            _pluginConfig = pluginConfig;
            _wallRevitRepository = wallRevitRepository;
            _localizationService = localizationService;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
            SelectLocationCommand = RelayCommand.Create<Window>(SelectLocation);
            ShowWallCommand = RelayCommand.Create<WallViewModel>(ShowWall, CanShowWall);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        public ICommand SelectLocationCommand { get; set; }
        public ICommand ShowWallCommand { get; set; }



        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public double Height {
            get => _height;
            set => this.RaiseAndSetIfChanged(ref _height, value);
        }

        public CustomLocation CustomLocation {
            get => _customLocation;
            set => this.RaiseAndSetIfChanged(ref _customLocation, value);
        }

        public ObservableCollection<WallViewModel> Walls {
            get => _walls;
            set => this.RaiseAndSetIfChanged(ref _walls, value);
        }

        public WallTypeViewModel WallType {
            get => _wallType;
            set => this.RaiseAndSetIfChanged(ref _wallType, value);
        }

        public ObservableCollection<WallTypeViewModel> WallTypes {
            get => _wallTypes;
            set => this.RaiseAndSetIfChanged(ref _wallTypes, value);
        }

        private void LoadView() {
            Walls = new ObservableCollection<WallViewModel>(
                _wallRevitRepository.GetWalls()
                    .Select(item => new WallViewModel(item)));

            WallTypes = new ObservableCollection<WallTypeViewModel>(
                _wallRevitRepository.GetWallTypes()
                    .Select(item => new WallTypeViewModel(item)));

            LoadConfig();
        }

        private void AcceptView() {
            SaveConfig();

            using(Transaction transaction = _wallRevitRepository.Document.StartTransaction("Create wall")) {
                Wall wall = _wallRevitRepository.CreateWall(CustomLocation, WallType.WallType, Height);
                Walls.Add(new WallViewModel(wall));

                transaction.Commit();
            }
        }

        private bool CanAcceptView() {
            if(Height < 0) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.HeightCheck");
                return false;
            }

            if(CustomLocation == null) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.CustomLocationCheck");
                return false;
            }

            if(WallType == null) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.WallTypeCheck");
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void SelectLocation(Window view) {
            view.Hide();
            try {
                XYZ start = null;
                XYZ finish = null;

                while(start == null || finish == null) {
                    if(start == null) {
                        start = _wallRevitRepository.PickPoint("Please select start point");
                    }

                    if(finish == null) {
                        finish = _wallRevitRepository.PickPoint("Please select finish point");
                    }

                    CustomLocation = new CustomLocation(start, finish);
                }
            } finally {
                view.ShowDialog();
            }
        }
        
        private void ShowWall(WallViewModel wallViewModel) {
            _wallRevitRepository.ShowElements(wallViewModel.Wall);
        }
        
        private bool CanShowWall(WallViewModel wallViewModel) {
            return wallViewModel != null;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_wallRevitRepository.Document);

            Height = setting?.Height ?? 20;
            WallType = WallTypes.FirstOrDefault(item => item.Id == setting?.WallTypeId)
                       ?? WallTypes.FirstOrDefault();
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_wallRevitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_wallRevitRepository.Document);

            setting.Height = Height;
            setting.WallTypeId = WallType.Id;

            _pluginConfig.SaveProjectConfig();
        }
    }
}
