using Autodesk.Revit.DB;

namespace RevitValueModifier.Models {
    internal class RevitParameter {

        public RevitParameter() {

        }

        public string Name { get; set; }
        public StorageType StorageType { get; set; }
        public bool IsBuiltin { get; set; }
    }
}
