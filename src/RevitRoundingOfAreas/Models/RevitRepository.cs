using System.Collections.Generic;

using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitRoundingOfAreas.Models;

internal class RevitRepository {
    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    /// <summary>
    /// Класс доступа к интерфейсу Revit.
    /// </summary>
    public UIApplication UIApplication { get; }

    /// <summary>
    /// Класс доступа к интерфейсу документа Revit.
    /// </summary>
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    /// <summary>
    /// Класс доступа к приложению Revit.
    /// </summary>
    public Application Application => UIApplication.Application;

    /// <summary>
    /// Класс доступа к документу Revit.
    /// </summary>
    public Document Document => ActiveUIDocument.Document;





    public IEnumerable<PhaseModel> GetPhaseModels() {
        var phases = Document.Phases;
        return phases
            .Cast<Phase>()
            .Select(phase => new PhaseModel {
                ElementId = phase.Id,
                Name = phase.Name
            });
    }

    public ElementId GetPhaseIdByName(string name) {
        return GetPhaseModels()
            .FirstOrDefault(phase => phase != null && phase.Name == name)
            ?.ElementId
            ?? ElementId.InvalidElementId;
    }


}
