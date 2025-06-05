using Autodesk.Revit.DB;

namespace RevitFinishing.ViewModels
{
    internal class RoomNameVM : SelectionElementVM {
        public RoomNameVM(string name, BuiltInParameter bltnParam) : base(name, bltnParam) {
        }
    }
}
