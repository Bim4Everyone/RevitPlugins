using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels
{
    internal class NoticeElementViewModel : BaseViewModel {
        private readonly Element _element;
        private readonly string _phaseName;
        private readonly string _levelName;

        public NoticeElementViewModel(Element element, string phaseName) {
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
