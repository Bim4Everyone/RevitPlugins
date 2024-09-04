using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;

namespace RevitAxonometryViews.Models {
    public class AxonometryConfig {
        private readonly string _sharedVisSystemName;
        private readonly string _systemName;
        private readonly ParameterElement _systemSharedNameParam;
        private readonly BuiltInParameter _systemNameBuiltInParam;
        private readonly ProjectParameters _projectParameters;

        internal AxonometryConfig(RevitRepository revitRepository) {
            _projectParameters = ProjectParameters.Create(revitRepository.Application);

            
            _projectParameters.SetupRevitParam(revitRepository.Document, SharedParamsConfig.Instance.VISSystemName);

            _systemSharedNameParam = revitRepository.Document.GetSharedParam("ФОП_ВИС_Имя системы");
            _systemNameBuiltInParam = BuiltInParameter.RBS_SYSTEM_NAME_PARAM;

            _sharedVisSystemName = _systemSharedNameParam.Name;
            _systemName = LabelUtils.GetLabelFor(_systemNameBuiltInParam);
        }

        public string SharedVisSystemName => _sharedVisSystemName;
        public string SystemName => _systemName;
        public BuiltInParameter SystemNameBuiltInParam => _systemNameBuiltInParam;
        public ParameterElement SystemSharedNameParam => _systemSharedNameParam;    

        public static List<BuiltInCategory> SystemCategories = new List<BuiltInCategory>() {
                BuiltInCategory.OST_DuctFitting,
                BuiltInCategory.OST_PipeFitting,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_DuctCurves,
                BuiltInCategory.OST_FlexDuctCurves,
                BuiltInCategory.OST_FlexPipeCurves,
                BuiltInCategory.OST_DuctTerminal,
                BuiltInCategory.OST_DuctAccessory,
                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_PlumbingFixtures,
                BuiltInCategory.OST_Sprinklers
            };
    }
}
