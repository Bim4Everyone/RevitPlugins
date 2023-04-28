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
#if REVIT_2020_OR_LESS
                if(_familyParameter.Definition.UnitType != UnitType.UT_Undefined) {
                    unitType = UnitUtils.GetTypeCatalogString(_familyParameter.Definition.UnitType);
                }
#elif REVIT_2021
                if(UnitUtils.IsSpec(_familyParameter.Definition.GetSpecTypeId())) {
                    unitType = UnitUtils.GetTypeCatalogStringForSpec(_familyParameter.Definition.GetSpecTypeId());
                }
#else
                if(UnitUtils.IsMeasurableSpec(_familyParameter.Definition.GetDataType())) {
                    unitType = UnitUtils.GetTypeCatalogStringForSpec(_familyParameter.Definition.GetDataType());
                }
#endif

                string displayUnitType = string.Empty;
#if REVIT_2020_OR_LESS
                try {
                    if(_familyParameter.DisplayUnitType != DisplayUnitType.DUT_UNDEFINED) {
                        displayUnitType = UnitUtils.GetTypeCatalogString(_familyParameter.DisplayUnitType);
                    }
                } catch {
                    if(StorageType == StorageType.Integer
                        || StorageType == StorageType.Double) {
                        
                        return "##NUMBER##GENERAL";
                    }
                    return "##OTHER##";
                }
#else
                try {
                    if(UnitUtils.IsUnit(_familyParameter.GetUnitTypeId())) {
                        displayUnitType = UnitUtils.GetTypeCatalogStringForUnit(_familyParameter.GetUnitTypeId());
                    }
                } catch {
                    if(StorageType == StorageType.Integer
                        || StorageType == StorageType.Double) {
                        
                        return "##NUMBER##GENERAL";
                    }

                    return "##OTHER##";
                }
#endif

                return $"##{unitType}##{displayUnitType}";
            }
        }

        public FamilyParamValuesViewModel FamilyParamValues { get; }
    }
}