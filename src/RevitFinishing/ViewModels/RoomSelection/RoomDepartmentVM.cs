using Autodesk.Revit.DB;

namespace RevitFinishing.ViewModels
{
    internal class RoomDepartmentVM : SelectionElementVM {
        public RoomDepartmentVM(string name, BuiltInParameter bltnParam) : base(name, bltnParam) {
        }
    }
}
