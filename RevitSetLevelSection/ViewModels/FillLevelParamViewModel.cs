using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class FillLevelParamViewModel : FillParamViewModel {
        private readonly RevitParam _revitParam;
        private readonly MainViewModel _mainViewModel;
        private readonly RevitRepository _revitRepository;

        private bool _isEnabled;

        public FillLevelParamViewModel(RevitParam revitParam, MainViewModel mainViewModel, RevitRepository revitRepository) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _revitParam = revitParam;
            _mainViewModel = mainViewModel;
            _revitRepository = revitRepository;
        }

        public override RevitParam RevitParam => _revitParam;
        public string Name => $"Обновить \"{RevitParam.Name}\"";

        public override bool IsEnabled {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }

        public override string GetErrorText() {
            if(!IsEnabled) {
                return null;
            }
            
            if(_mainViewModel.LinkType?.HasAreaScheme == false) {
                return "Выбранная связь не содержит схему зонирования.";
            }
            
            if(_mainViewModel.LinkType?.HasAreas == false) {
                return "Выбранная связь не содержит зоны.";
            }

            return null;
        }

        public override void UpdateElements() {
            _revitRepository.SetLevelParam(RevitParam);
        }

        public override ParamSettings GetParamSettings() {
            return new ParamSettings() {
                IsEnabled = IsEnabled, 
                ParamId = RevitParam.Id
            };
        }

        public override void SetParamSettings(ParamSettings paramSettings) {
            IsEnabled = paramSettings.IsEnabled;
        }
    }
}
