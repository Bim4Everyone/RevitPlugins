using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.ViewModels.Interfaces;

namespace RevitLintelPlacement.ViewModels {
    internal class MaterialClassesConditionViewModel : BaseViewModel, IConditionViewModel {

        private readonly RevitRepository _revitRepository;
        private ObservableCollection<MaterialClassConditionViewModel> _materialClassConditions;

        public ObservableCollection<MaterialClassConditionViewModel> MaterialClassConditions { 
            get => _materialClassConditions; 
            set => this.RaiseAndSetIfChanged(ref _materialClassConditions, value); 
        }

        public MaterialClassesConditionViewModel() {

        }

        public MaterialClassesConditionViewModel(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
        }

        public bool Check(FamilyInstance elementInWall) {

            if(elementInWall == null || elementInWall.Id == ElementId.InvalidElementId)
                throw new ArgumentNullException(nameof(elementInWall));

            if(elementInWall.Host == null || elementInWall.Host.GetType() != typeof(Wall))
                throw new ArgumentNullException(nameof(elementInWall), "На проверку передан некорректный элемент.");

            var materials = _revitRepository.GetElements(elementInWall.Host.GetMaterialIds(false)); //TODO: может быть и true, проверить
            foreach(var m in materials) {
                if(MaterialClassConditions.Any(mc => mc.IsChecked && mc.Name.Equals(((Material)m).MaterialClass, StringComparison.InvariantCultureIgnoreCase))) //TODO: для английской версии дожен быть config
                    return true;
            }
            return false;
        }
    }

   
}
