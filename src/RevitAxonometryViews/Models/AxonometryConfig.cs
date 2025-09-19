using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;

namespace RevitAxonometryViews.Models;
public class AxonometryConfig {
    private readonly ProjectParameters _projectParameters;

    internal AxonometryConfig(Document document) {
        _projectParameters = ProjectParameters.Create(document.Application);
        _projectParameters.SetupRevitParam(document, SharedParamsConfig.Instance.VISSystemName);

        SystemSharedNameParam = SharedParamsConfig.Instance.VISSystemName.GetRevitParamElement(document);
        SystemNameBuiltInParam = BuiltInParameter.RBS_SYSTEM_NAME_PARAM;

        SharedVisSystemName = SystemSharedNameParam.Name;
        SystemName = LabelUtils.GetLabelFor(SystemNameBuiltInParam);
    }

    public string SharedVisSystemName { get; }
    public string SystemName { get; }
    public BuiltInParameter SystemNameBuiltInParam { get; }
    public ParameterElement SystemSharedNameParam { get; }
    public static IReadOnlyCollection<BuiltInCategory> SystemCategories { get; } = [
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
    ];
}
