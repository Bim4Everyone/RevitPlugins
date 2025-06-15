using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitFinishing.ViewModels
{
    internal class RoomNameVM : SelectionElementVM {
        public RoomNameVM(string name, BuiltInParameter bltnParam, ILocalizationService localizationService) 
            : base(name, bltnParam, localizationService) {
        }
    }
}
