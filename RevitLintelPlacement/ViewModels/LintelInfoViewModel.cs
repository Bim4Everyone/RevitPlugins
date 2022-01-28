using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class LintelInfoViewModel : BaseViewModel {
        private readonly RevitRepository revitRepository;
        private string elementInWallName;
        private ElementId _lintelId;
        private ElementId _elementInWallId;
        private string wallTypeName;

        public LintelInfoViewModel() {

        }

        public LintelInfoViewModel(RevitRepository _revitRepository) {
            this.revitRepository = _revitRepository;

        }

        public string ElementInWallName {
            get => elementInWallName;
            set => this.RaiseAndSetIfChanged(ref elementInWallName, value);
        }

        public ElementId LintelId {
            get => _lintelId;
            set => this.RaiseAndSetIfChanged(ref _lintelId, value);
        }

        public ElementId ElementInWallId {
            get => _elementInWallId;
            set => this.RaiseAndSetIfChanged(ref _elementInWallId, value);
        }

        public string WallTypeName {
            get => wallTypeName;
            set => this.RaiseAndSetIfChanged(ref wallTypeName, value); 
        }
    }
}
