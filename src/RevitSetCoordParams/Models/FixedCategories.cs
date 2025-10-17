using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models;

internal class FixedCategories {

    private readonly BuiltInCategory[] _allCategories = [
    BuiltInCategory.OST_CableTray,
    BuiltInCategory.OST_Casework,
    BuiltInCategory.OST_Ceilings,
    BuiltInCategory.OST_Curtain_Systems,
    BuiltInCategory.OST_CurtainWallMullions,
    BuiltInCategory.OST_CurtainWallPanels,
    BuiltInCategory.OST_CommunicationDevices,
    BuiltInCategory.OST_Conduit,
    BuiltInCategory.OST_CurtainWallPanels,
    BuiltInCategory.OST_Doors,
    BuiltInCategory.OST_DuctCurves,
    BuiltInCategory.OST_DuctFitting,
    BuiltInCategory.OST_DuctAccessory,
    BuiltInCategory.OST_DuctInsulations,
    BuiltInCategory.OST_DuctTerminal,
    BuiltInCategory.OST_ElectricalEquipment,
    BuiltInCategory.OST_ElectricalFixtures,
    BuiltInCategory.OST_Floors,
    BuiltInCategory.OST_Furniture,
    BuiltInCategory.OST_FurnitureSystems,
    BuiltInCategory.OST_GenericModel,
    BuiltInCategory.OST_LightingDevices,
    BuiltInCategory.OST_LightingFixtures,
    BuiltInCategory.OST_Mass,
    BuiltInCategory.OST_MechanicalEquipment,
    BuiltInCategory.OST_PlaceHolderPipes,
    BuiltInCategory.OST_PipeCurves,
    BuiltInCategory.OST_PipeFitting,
    BuiltInCategory.OST_PipeAccessory,
    BuiltInCategory.OST_PipeInsulations,
    BuiltInCategory.OST_Parking,
    BuiltInCategory.OST_Planting,
    BuiltInCategory.OST_PlumbingFixtures,
    BuiltInCategory.OST_RailingSystemRail,
    BuiltInCategory.OST_Railings,
    BuiltInCategory.OST_Ramps,
    BuiltInCategory.OST_Roads,
    BuiltInCategory.OST_Roofs,
    BuiltInCategory.OST_SecurityDevices,
    BuiltInCategory.OST_Sprinklers,
    BuiltInCategory.OST_Stairs,
    BuiltInCategory.OST_StairsRailing,
    BuiltInCategory.OST_StructuralColumns,
    BuiltInCategory.OST_StructuralFoundation,
    BuiltInCategory.OST_StructuralFraming,
    BuiltInCategory.OST_Topography,
    BuiltInCategory.OST_Walls,
    BuiltInCategory.OST_Windows];

    public IEnumerable<BuiltInCategory> GetDefaultBuiltInCategories() {
        return _allCategories;
    }
}

