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
using dosymep.Revit;

namespace RevitAxonometryViews.Models {
    public class AxonometryConfig {
        private readonly string _sharedVisSystemName;
        private readonly string _systemName;
        private readonly ParameterElement _systemSharedNameParam;
        private readonly BuiltInParameter _systemNameBuiltInParam;

        public AxonometryConfig(Document document) {
            //ParameterElement systemNameParam = document.GetProjectParam("Имя системы");
            _systemSharedNameParam = document.GetSharedParam("ФОП_ВИС_Имя системы");
            _systemNameBuiltInParam = BuiltInParameter.RBS_SYSTEM_NAME_PARAM;

            _sharedVisSystemName = _systemSharedNameParam.Name;
            _systemName = LabelUtils.GetLabelFor(_systemNameBuiltInParam);

        }
        //Нужно это сделать через SharedParameterConfig

        // public SharedParam SharedSystemParameter = SharedParamsConfig.Instance.ApartmentArea;
        


        //Добавляем через параметры
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
                BuiltInCategory.OST_Sprinklers,
                BuiltInCategory.OST_GenericModel
            };
    }
}
