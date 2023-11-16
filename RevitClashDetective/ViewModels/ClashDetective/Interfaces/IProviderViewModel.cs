using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.ViewModels.ClashDetective.Interfaces {
    interface IProviderViewModel {
        string Name { get; }
        IProvider GetProvider(Document doc, Transform transform);
    }
}
