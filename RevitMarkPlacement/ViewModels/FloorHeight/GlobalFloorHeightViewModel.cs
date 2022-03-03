using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels {

    internal class GlobalFloorHeightViewModel : BaseViewModel, IFloorHeightProvider {
        private readonly RevitRepository _revitRepository;

        public GlobalFloorHeightViewModel(RevitRepository revitRepository, string description) {
            this._revitRepository = revitRepository;
            GlobalParameters = _revitRepository.GetGlobalParameters().ToList();
            SelectedGlobalParameter = GlobalParameters[0];
            Description = description;
        }
        public string Description { get; }
        public GlobalParameterViewModel SelectedGlobalParameter { get; set; }
        public List<GlobalParameterViewModel> GlobalParameters { get; }
        public double GetFloorHeight() {
            return SelectedGlobalParameter.Value;
        }
    }
}
