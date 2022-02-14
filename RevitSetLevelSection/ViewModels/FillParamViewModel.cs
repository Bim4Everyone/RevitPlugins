
using System;
using System.Collections;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitSetLevelSection.Models;

namespace RevitSetLevelSection.ViewModels {
    internal class FillParamViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private bool _isEnabled;
        private DesignOptionsViewModel _designOptions;
        
        private string _paramValue;
        private RevitParam _revitParam;

        public FillParamViewModel(RevitRepository revitRepository) {
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

        public void UpdateElements(bool fromRevitParam) {
            if(fromRevitParam) {
                _revitRepository.ProjectInfo.SetParamValue(RevitParam, ParamValue);
                Parameter parameter = _revitRepository.ProjectInfo.GetParam(RevitParam);
                IEnumerable<Element> elements = _revitRepository.GetElements(RevitParam);
                _revitRepository.UpdateElements(parameter, RevitParam, elements);
            } else {
                foreach(FamilyInstance massObject in DesignOptions.GetMassObjects()) {
                    IEnumerable<Element> elements = _revitRepository.GetElements(massObject, RevitParam);
                    _revitRepository.UpdateElements(massObject, RevitParam, elements);
                }
            }
        }
    }
}