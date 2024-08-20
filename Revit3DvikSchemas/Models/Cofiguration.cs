using System;
using System.Collections.Generic;
using System.Text;

using dosymep.Revit;

namespace Revit3DvikSchemas.Models
{
    static class Cofiguration
    {
        public static List<BuiltInCategory> SystemAndFopCats = new List<BuiltInCategory>() {
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
                BuiltInCategory.OST_Sprinklers,
                BuiltInCategory.OST_GenericModel
            };

        public const string FopVisSysName = "ФОП_ВИС_Имя системы";
        public const string SysName = "Имя системы"; 

    }
}
