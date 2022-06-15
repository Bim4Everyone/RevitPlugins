using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models {
    internal interface IProvider {
        Document Doc { get; }
        /// <summary>
        /// Преобразование относительно открытого файла
        /// </summary>
        Transform MainTransform { get; }
        List<Element> GetElements();
        List<Solid> GetSolids(Element Element);
    }
}
