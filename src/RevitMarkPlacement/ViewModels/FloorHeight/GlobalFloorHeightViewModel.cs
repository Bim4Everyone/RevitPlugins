using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels {

    internal class GlobalFloorHeightViewModel : BaseViewModel, IFloorHeightProvider {
        private readonly RevitRepository _revitRepository;
        private readonly AnnotationsSettings _settings;
        private bool _isEnabled;
        private GlobalParameterViewModel _selectedGlobalParameter;

        public GlobalFloorHeightViewModel(RevitRepository revitRepository, string description, AnnotationsSettings settings) {
            _revitRepository = revitRepository;

            Description = description;
            _settings = settings;
            GlobalParameters = _revitRepository.GetDoubleGlobalParameters()
                .Select(item => new GlobalParameterViewModel(item.Name, GetValue(item), item.Id))
                .ToList();
            if(GlobalParameters.Count > 0) {
                var configGlobalParameter = _revitRepository.GetElement(_settings.GlobalParameterId) as GlobalParameter;
                if(configGlobalParameter == null) {
                    SelectedGlobalParameter = GlobalParameters[0];
                } else {
                    SelectedGlobalParameter = GlobalParameters.FirstOrDefault(item => item.ElementId == configGlobalParameter.Id) ?? GlobalParameters[0];
                }
                IsEnabled = true;
            } else {
                IsEnabled = false;
            }
        }

        public bool IsEnabled {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }

        public string Description { get; }
        public List<GlobalParameterViewModel> GlobalParameters { get; }

        public GlobalParameterViewModel SelectedGlobalParameter {
            get => _selectedGlobalParameter;
            set => this.RaiseAndSetIfChanged(ref _selectedGlobalParameter, value);
        }

        public string GetFloorHeight() {
            return SelectedGlobalParameter?.Value.ToString();
        }

        private double GetValue(GlobalParameter parameter) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertFromInternalUnits((parameter.GetValue() as DoubleParameterValue).Value, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertFromInternalUnits((parameter.GetValue() as DoubleParameterValue).Value, UnitTypeId.Millimeters);
#endif
        }
    }
}
