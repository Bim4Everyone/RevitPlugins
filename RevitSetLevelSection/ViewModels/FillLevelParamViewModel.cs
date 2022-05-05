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
        private readonly RevitRepository _revitRepository;

        private bool _isEnabled;

        public FillLevelParamViewModel(RevitRepository revitRepository) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _revitRepository = revitRepository;
        }

        public override RevitParam RevitParam { get; set; }
        public string Name => $"Обновить \"{RevitParam.Name}\"";

        public override bool IsEnabled {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }

        public override string GetErrorText(bool fromRevitParam) {
            return null;
        }

        public override void UpdateElements(bool fromProjectParam) {
            _revitRepository.SetLevelParam(RevitParam);
        }

        public override ParamSettings GetParamSettings() {
            return new ParamSettings() {
                IsEnabled = IsEnabled, 
                PropertyName = RevitParam.Id
            };
        }

        public override void SetParamSettings(ParamSettings paramSettings) {
            IsEnabled = paramSettings.IsEnabled;
        }
    }
}
