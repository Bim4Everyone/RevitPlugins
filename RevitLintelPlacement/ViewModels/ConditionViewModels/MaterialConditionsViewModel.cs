using System;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class MaterialConditionsViewModel : BaseViewModel, IConditionViewModel {
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<MaterialConditionViewModel> _materialConditions;

        public MaterialConditionsViewModel() {

        }

        public MaterialConditionsViewModel(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
        }


        public ObservableCollection<MaterialConditionViewModel> MaterialConditions {
            get => _materialConditions;
            set => this.RaiseAndSetIfChanged(ref _materialConditions, value);
        }


        public bool Check(FamilyInstance elementInWall) {

            if(elementInWall == null || elementInWall.Id == ElementId.InvalidElementId)
                throw new ArgumentNullException(nameof(elementInWall));

            if(elementInWall.Host == null || !(elementInWall.Host is Wall wall))
                return false;
                //throw new ArgumentNullException(nameof(elementInWall), "На проверку передан некорректный элемент.");

            var materials = _revitRepository.GetElements(wall.GetMaterialIds(true)); //TODO: может быть и true, проверить
            foreach(var m in materials) {
                //if(MaterialConditions.Any(mc => mc.IsChecked && mc.Name.Equals(((Material) m).Name, StringComparison.CurrentCultureIgnoreCase)))
                if(MaterialConditions.Any(mc => mc.IsChecked && ((Material) m).Name.ToLower().Contains(mc.Name.ToLower())))
                    return true;
            }
            return false;
        }
    }
}
