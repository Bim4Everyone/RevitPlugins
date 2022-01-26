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
                throw new ArgumentNullException("На проверку не передан элемент.");

            if(elementInWall.Host == null || elementInWall.Host.GetType() != typeof(Wall))
                throw new ArgumentNullException("На проверку передан некорректный элемент.");

            return WallTypes.Any(wt => ((Wall) elementInWall.Host).WallType.Name.Contains(wt.Name)); //TODO: или равно (у Арсения уточнить)
        }
    }
}
