using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitExamplePlugin.ViewModels {
    internal sealed class WallViewModel : BaseViewModel {
        public WallViewModel(Wall wall) {
            Wall = wall;
        }
        
        public Wall Wall { get; }

        public ElementId Id => Wall.Id;
        public string Name => Wall.Name;
    }
}
