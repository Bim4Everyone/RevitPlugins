using Autodesk.Revit.DB;

namespace RevitListOfSchedules.ViewModels {
    public class GroupParameterViewModel {

        private readonly ElementId _parameterId;
        private readonly Document _document;

        public GroupParameterViewModel(Document document, ElementId parameterId) {

            _parameterId = parameterId;
            _document = document;

            Name = GetParamName();
        }

        public string Name { get; set; }

        public string GetParamName() {
            if(_parameterId != null) {
                return _document.GetElement(_parameterId).Name;
            } else {
                return "< Нет параметра группировки >";
            }
        }
    }
}
