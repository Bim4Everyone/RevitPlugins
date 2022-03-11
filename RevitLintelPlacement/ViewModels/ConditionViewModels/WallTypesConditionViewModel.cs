using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class WallTypesConditionViewModel : BaseViewModel, IConditionViewModel {
        private ObservableCollection<WallTypeConditionViewModel> _wallTypes;

        public ObservableCollection<WallTypeConditionViewModel> WallTypes {
            get => _wallTypes;
            set => this.RaiseAndSetIfChanged(ref _wallTypes, value);
        }

        public bool Check(FamilyInstance elementInWall) {
            if(elementInWall == null || elementInWall.Id == ElementId.InvalidElementId)
                throw new ArgumentNullException(nameof(elementInWall));

            if(elementInWall.Host == null || !(elementInWall.Host is Wall wall))
                return false;

            return WallTypes.Any(wt => (wall.WallType.Name.ToLower().Contains(wt.Name.ToLower())));
        }
    }
}
