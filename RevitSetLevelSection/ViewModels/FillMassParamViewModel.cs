
using System;
using System.Collections;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class FillMassParamViewModel : FillParamViewModel {
        private readonly RevitRepository _revitRepository;

        private bool _isEnabled;
        private DesignOptionsViewModel _designOptions;

        private string _paramValue;
        private RevitParam _revitParam;

        public FillMassParamViewModel(RevitRepository revitRepository) {
            if(revitRepository is null) {
                throw new ArgumentNullException(nameof(revitRepository));
            }

            _revitRepository = revitRepository;
        }

        public RevitParam RevitParam {
            get => _revitParam;
            set {
                this.RaiseAndSetIfChanged(ref _revitParam, value);
                ParamValue = (string) _revitRepository.ProjectInfo.GetParamValueOrDefault(RevitParam);
            }
        }

        public string Name => $"Заполнить \"{RevitParam.Name}\"";

        public string ParamValue {
            get => _paramValue;
            set => this.RaiseAndSetIfChanged(ref _paramValue, value);
        }

        public bool IsEnabled {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }

        public DesignOptionsViewModel DesignOptions {
            get => _designOptions;
            set => this.RaiseAndSetIfChanged(ref _designOptions, value);
        }

        public override void UpdateElements(bool fromRevitParam) {
            if(fromRevitParam) {
                _revitRepository.UpdateElements(RevitParam, ParamValue);
            } else {
                _revitRepository.UpdateElements(RevitParam, DesignOptions.GetMassObjects());
            }
        }
    }
}