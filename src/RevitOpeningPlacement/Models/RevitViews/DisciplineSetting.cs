using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RevitViews {
    /// <summary>
    /// Класс, предоставляющий настройки дисциплины 3D вида
    /// </summary>
    internal class DisciplineSetting : IView3DSetting {
        /// <summary>
        /// Конструктор класса, предоставляющего настройки дисциплины 3D вида
        /// </summary>
        public DisciplineSetting() { }

        public void Apply(View3D view3D) {
            view3D.Discipline = ViewDiscipline.Coordination;
        }
    }
}
