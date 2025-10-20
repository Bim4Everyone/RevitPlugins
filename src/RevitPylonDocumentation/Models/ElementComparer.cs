using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models;
internal class ElementComparer : IEqualityComparer<Element> {
    public bool Equals(Element x, Element y) {
        // Если оба объекта null, они равны
        if(x == null && y == null)
            return true;

        // Если один из объектов null, они не равны
        if(x == null || y == null)
            return false;

        // Сравниваем по свойству Id
        return x.Id == y.Id;
    }

    public int GetHashCode(Element obj) {
        // Используем Id для получения Hash-кода
        return obj.Id.GetHashCode();
    }
}
