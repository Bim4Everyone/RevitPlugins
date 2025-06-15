using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitFinishing.ViewModels
{
    internal class RoomDepartmentVM : SelectionElementVM {
        public RoomDepartmentVM(string name, BuiltInParameter bltnParam, ILocalizationService localizationService) 
            : base(name, bltnParam, localizationService) {
        }
    }
}
