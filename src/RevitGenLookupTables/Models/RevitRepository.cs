using System;
using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitGenLookupTables.Models;

internal class RevitRepository {
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

    public string DocumentTitle => Document.Title;
    public bool IsFamilyDocument => Document.IsFamilyDocument;

    public Family GetMainFamily() {
        return Document.IsFamilyDocument
            ? Document.OwnerFamily
            : throw new InvalidOperationException("Документ должен быть семейством.");
    }

    public IList<FamilyParameter> GetFamilyParams() {
        return Document.FamilyManager.GetParameters();
    }

    public string GetColumnMetadata(FamilyParameter familyParameter) {
        string unitType = GetUnitType(familyParameter);
        string displayUnitData = GetDisplayUnitData(familyParameter);

        return string.IsNullOrEmpty(displayUnitData)
            ? $"##{unitType}##{displayUnitData}"
            : GetDefaultColumnMetadata(familyParameter);
    }

    private static string GetDisplayUnitData(FamilyParameter familyParameter) {
#if REVIT_2020_OR_LESS
        try {
            if(familyParameter.DisplayUnitType != DisplayUnitType.DUT_UNDEFINED) {
                return UnitUtils.GetTypeCatalogString(familyParameter.DisplayUnitType);
            }
        } catch {
            return null;
        }
#else
        try {
            if(UnitUtils.IsUnit(familyParameter.GetUnitTypeId())) {
                return UnitUtils.GetTypeCatalogStringForUnit(familyParameter.GetUnitTypeId());
            }
        } catch {
            return null;
        }
#endif
        return null;
    }

    private static string GetUnitType(FamilyParameter familyParameter) {
#if REVIT_2020_OR_LESS
        if(familyParameter.Definition.UnitType != UnitType.UT_Undefined) {
            return UnitUtils.GetTypeCatalogString(familyParameter.Definition.UnitType);
        }
#elif REVIT_2021
        if(UnitUtils.IsSpec(familyParameter.Definition.GetSpecTypeId())) {
            return UnitUtils.GetTypeCatalogStringForSpec(familyParameter.Definition.GetSpecTypeId());
        }
#else
        if(UnitUtils.IsMeasurableSpec(familyParameter.Definition.GetDataType())) {
            return UnitUtils.GetTypeCatalogStringForSpec(familyParameter.Definition.GetDataType());
        }
#endif
        return "OTHER";
    }

    private static string GetDefaultColumnMetadata(FamilyParameter familyParameter) {
        return familyParameter.StorageType is StorageType.Integer or StorageType.Double
            ? "##NUMBER##GENERAL"
            : "##OTHER##";
    }
}
