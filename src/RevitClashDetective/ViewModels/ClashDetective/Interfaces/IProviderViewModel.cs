using Autodesk.Revit.DB;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.ViewModels.ClashDetective.Interfaces;
internal interface IProviderViewModel : IName {
    IProvider GetProvider(Document doc, Transform transform);
}
