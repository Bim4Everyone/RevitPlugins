using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models {
    internal interface IDesignOption {
        ElementId Id { get; }
        string Name { get; }
    }

    internal class DefaultDesignOption : IDesignOption {
        public ElementId Id => ElementId.InvalidElementId;
        public string Name => "Главная модель";
    }

    internal class DesignOptionAdapt : IDesignOption {
        private readonly DesignOption _designOption;

        public DesignOptionAdapt(DesignOption designOption) {
            _designOption = designOption;
        }

        public ElementId Id => _designOption.Id;
        public string Name => _designOption.Name;
    }
}