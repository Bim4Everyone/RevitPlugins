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

        private static readonly IReadOnlyCollection<BuiltInCategory> _systemCategories = new List<BuiltInCategory>() {
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

        internal AxonometryConfig(Document document) {
            _projectParameters = ProjectParameters.Create(document.Application);
            _projectParameters.SetupRevitParam(document, SharedParamsConfig.Instance.VISSystemName);

            _systemSharedNameParam = SharedParamsConfig.Instance.VISSystemName.GetRevitParamElement(document);
            _systemNameBuiltInParam = BuiltInParameter.RBS_SYSTEM_NAME_PARAM;

            _sharedVisSystemName = _systemSharedNameParam.Name;
            _systemName = LabelUtils.GetLabelFor(_systemNameBuiltInParam);
        }

        public string SharedVisSystemName => _sharedVisSystemName;
        public string SystemName => _systemName;
        public BuiltInParameter SystemNameBuiltInParam => _systemNameBuiltInParam;
        public ParameterElement SystemSharedNameParam => _systemSharedNameParam;

        public static IReadOnlyCollection<BuiltInCategory> SystemCategories => _systemCategories;
    }
}
