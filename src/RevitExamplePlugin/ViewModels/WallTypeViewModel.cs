using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitExamplePlugin.ViewModels {
    internal sealed class WallTypeViewModel : BaseViewModel {
        public WallTypeViewModel(WallType wallType) {
            WallType = wallType;
        }
        
        public WallType WallType { get; }

        public ElementId Id => WallType.Id;
        public string Name => WallType.Name;
    }
}
