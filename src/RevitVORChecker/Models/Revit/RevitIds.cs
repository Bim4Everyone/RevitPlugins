using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitVORChecker.Models.Revit;

/// <summary>
/// Значения Id элементов для Revit 2022–2024.
/// </summary>
internal static class RevitIds {
    public static long GetValue(ElementId id) {
        return id.GetIdValue();
    }

    public static ElementId Create(long value) {
#if REVIT_2023_OR_LESS
        return new ElementId((int) value);
#else
        return new ElementId(value);
#endif
    }
}
