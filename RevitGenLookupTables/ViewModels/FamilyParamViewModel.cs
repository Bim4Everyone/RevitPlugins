using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitGenLookupTables.Models;

namespace RevitGenLookupTables.ViewModels {
    internal class FamilyParamViewModel : BaseViewModel {
        private readonly FamilyParameter _familyParameter;
        private readonly RevitRepository _revitRepository;

        public FamilyParamViewModel(RevitRepository revitRepository, FamilyParameter familyParameter) {
            _revitRepository = revitRepository;
            _familyParameter = familyParameter;

            FamilyParamValues = new FamilyParamValuesViewModel(revitRepository, _familyParameter.StorageType);
        }

        public string Name {
            get { return _familyParameter.Definition.Name; }
        }

        public StorageType StorageType {
            get { return _familyParameter.StorageType; } 
        }

        public string ColumnMetaData {
            get {
                string unitType = "OTHER";
                if(_familyParameter.Definition.UnitType != UnitType.UT_Undefined) {
                    unitType = UnitUtils.GetTypeCatalogString(_familyParameter.Definition.UnitType);
                }

                string displayUnitType = string.Empty;
                try {
                    if(_familyParameter.DisplayUnitType != DisplayUnitType.DUT_UNDEFINED) {
                        displayUnitType = UnitUtils.GetTypeCatalogString(_familyParameter.DisplayUnitType);
                    }
                } catch {
                    displayUnitType = "GENERAL";
                }

                return $"##{unitType}##{displayUnitType}";
            }
        }

        public FamilyParamValuesViewModel FamilyParamValues { get; }
    }
}