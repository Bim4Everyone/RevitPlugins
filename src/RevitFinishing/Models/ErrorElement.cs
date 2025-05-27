using Autodesk.Revit.DB;

namespace RevitFinishing.Models
{
    internal class ErrorElement {
        private readonly Element _element;
        private readonly string _phaseName;
        private readonly string _levelName;

        public ErrorElement(Element element, string phaseName) {
            _element = element;
            _phaseName = phaseName;

            ElementId levelId = _element.LevelId;
            _levelName = levelId != ElementId.InvalidElementId
                ? _element.Document.GetElement(levelId).Name : "Без уровня";
        }

        public ElementId ElementId => _element.Id;
        public string ElementName => _element.Name;
        public string CategoryName => _element.Category.Name;
        public string PhaseName => _phaseName;
        public string LevelName => _levelName;
    }
}
