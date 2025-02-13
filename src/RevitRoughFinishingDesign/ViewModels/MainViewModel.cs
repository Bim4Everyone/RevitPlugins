using System.Collections.Generic;
using System.Windows.Input;

using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoughFinishingDesign.Models;
using RevitRoughFinishingDesign.Services;

namespace RevitRoughFinishingDesign.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;
        private readonly ICurveLoopsSimplifier _curveLoopsSimplifier;
        private readonly WallDesignDataGetter _wallDesignDataGetter;
        private readonly CreatesLinesForFinishing _createsLinesForFinishing;
        private string _errorText;
        private string _saveProperty;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService,
            ICurveLoopsSimplifier curveLoopsSimplifier,
            WallDesignDataGetter wallDesignDataGetter,
            CreatesLinesForFinishing createsLinesForFinishing) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;
            _curveLoopsSimplifier = curveLoopsSimplifier;
            _wallDesignDataGetter = wallDesignDataGetter;
            _createsLinesForFinishing = createsLinesForFinishing;
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        private string _lineOffset;
        public string LineOffset {
            get => _lineOffset;
            set {
                RaiseAndSetIfChanged(ref _lineOffset, value);
                OnPropertyChanged(nameof(ErrorText));
            }
        }
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }

        private void LoadView() {
            LoadConfig();
        }

        private void AcceptView() {
            IList<Room> rooms = _revitRepository.GetTestRooms();
            using(var transaction = _revitRepository.Document.StartTransaction("Тест")) {
                _createsLinesForFinishing.DrawLines(_pluginConfig);
                //foreach(Room room in rooms) {
                //    IList<WallDesignData> wallDesignData = _wallDesignDataGetter.GetWallDesignDatas();
                //RevitRoomHandler roomHandler = new RevitRoomHandler(_revitRepository, room, _curveLoopsSimplifier);
                //roomHandler.CreateTestLines();
                //}
                transaction.Commit();
            }
            SaveConfig();
        }

        private bool CanAcceptView() {
            if(!double.TryParse(LineOffset, out double result)) {
                ErrorText = "Смещение не должно содержать символов или букв";
                return false;
            }
            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            LineOffset = _pluginConfig.LineOffset.GetValueOrDefault(0).ToString();

            SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainWindow.Hello");
            OnPropertyChanged(nameof(ErrorText));
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);

            _pluginConfig.LineOffset = double.Parse(LineOffset);

            setting.SaveProperty = SaveProperty;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
