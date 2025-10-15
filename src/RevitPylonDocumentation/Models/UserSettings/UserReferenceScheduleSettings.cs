using Autodesk.Revit.DB;

namespace RevitPylonDocumentation.Models.UserSettings;
internal class UserReferenceScheduleSettings {
    /// <summary>
    /// Эталонная спецификация материалов
    /// </summary>
    public ViewSchedule ReferenceMaterialSchedule { get; set; }

    /// <summary>
    /// Эталонная ведомость деталей для системной арматуры
    /// </summary>
    public ViewSchedule ReferenceSystemPartsSchedule { get; set; }

    /// <summary>
    /// Эталонная ведомость деталей для IFC арматуры
    /// </summary>
    public ViewSchedule ReferenceIfcPartsSchedule { get; set; }

    /// <summary>
    /// Эталонная спецификация арматуры
    /// </summary>
    public ViewSchedule ReferenceSkeletonSchedule { get; set; }

    /// <summary>
    /// Эталонная спецификация арматуры
    /// </summary>
    public ViewSchedule ReferenceSkeletonByElemsSchedule { get; set; }
}
