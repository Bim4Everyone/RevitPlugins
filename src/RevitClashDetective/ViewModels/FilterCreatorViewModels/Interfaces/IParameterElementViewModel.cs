using Autodesk.Revit.DB;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels.Interfaces {
    internal interface IParameterElementViewModel {
        string Name { get; }
        StorageType StorageType { get; }
    }
}
