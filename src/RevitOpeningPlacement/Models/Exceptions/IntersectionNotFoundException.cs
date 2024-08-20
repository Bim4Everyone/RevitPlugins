using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Exceptions {
    internal class IntersectionNotFoundException : Exception {
        public IntersectionNotFoundException(string message) : base(message) { }
        public static IntersectionNotFoundException GetException(Element e1, Element e2) {
            return new IntersectionNotFoundException($"Не удалось найти пересечение между следующими элементами:" +
                $" \"{e1.Id}\" в файле \"{e1.Document.Title}\" и \"{e2.Id}\" в файле \"{e2.Document.Title}\"");
        }
    }
}
