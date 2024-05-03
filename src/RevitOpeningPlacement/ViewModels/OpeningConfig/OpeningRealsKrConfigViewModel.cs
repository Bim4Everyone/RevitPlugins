using System;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class OpeningRealsKrConfigViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        public OpeningRealsKrConfigViewModel(RevitRepository revitRepository, OpeningRealsKrConfig openingRealsKrConfig) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            if(openingRealsKrConfig is null) {
                throw new ArgumentNullException(nameof(openingRealsKrConfig));
            }

            PlaceByMep = openingRealsKrConfig.PlacementType == OpeningRealKrPlacementType.PlaceByMep;
            PlaceByAr = !PlaceByMep;

            SaveConfigCommand = RelayCommand.Create(SaveConfig);
        }


        private bool _placeByMep;

        public bool PlaceByMep {
            get => _placeByMep;
            set => RaiseAndSetIfChanged(ref _placeByMep, value);
        }


        private bool _placeByAr;

        public bool PlaceByAr {
            get => _placeByAr;
            set => RaiseAndSetIfChanged(ref _placeByAr, value);
        }


        public ICommand SaveConfigCommand { get; }

        private void SaveConfig() {
            GetOpeningConfig().SaveProjectConfig();
        }


        private OpeningRealsKrConfig GetOpeningConfig() {
            var config = OpeningRealsKrConfig.GetOpeningConfig(_revitRepository.Doc);
            config.PlacementType = PlaceByAr
                ? OpeningRealKrPlacementType.PlaceByAr
                : OpeningRealKrPlacementType.PlaceByMep;
            return config;
        }
    }
}
