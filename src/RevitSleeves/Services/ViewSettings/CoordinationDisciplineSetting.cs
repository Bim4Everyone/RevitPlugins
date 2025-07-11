using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitSleeves.Services.ViewSettings;
internal class CoordinationDisciplineSetting : IView3DSetting {
    /// <summary>
    /// Конструктор класса, предоставляющего настройки дисциплины 3D вида
    /// </summary>
    public CoordinationDisciplineSetting() { }


    public void Apply(View3D view3D) {
        view3D.Discipline = ViewDiscipline.Coordination;
    }
}
