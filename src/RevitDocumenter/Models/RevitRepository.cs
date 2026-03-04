using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitDocumenter.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// В случае если данный класс разрастается, рекомендуется его разделить на несколько.
/// </remarks>
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

    /// <summary>
    /// Возвращает список типоразмеров размеров
    /// </summary>
    public List<DimensionType> DimensionTypes => new FilteredElementCollector(Document)
        .OfClass(typeof(DimensionType))
        .OfType<DimensionType>()
        .Where(d => d.StyleType == DimensionStyleType.Linear)
        .OrderBy(a => a.FamilyName)
        .ThenBy(a => a.Name)
        .ToList();

    public List<Grid> GetGrids() {
        return new FilteredElementCollector(Document)
          .OfClass(typeof(Grid))
          .WhereElementIsNotElementType()
          .OfType<Grid>()
          .ToList();
    }

    public List<RebarElement> GetRebarElements(
        string familyNamePart,
        List<string> verticalRefNames,
        List<string> horizontalRefNames) => new FilteredElementCollector(Document)
          .OfCategory(BuiltInCategory.OST_Rebar)
          .WhereElementIsNotElementType()
          .OfType<FamilyInstance>()
          .Where(r => {
              var type = Document.GetElement(r.GetTypeId()) as ElementType;
              return type != null &&
                     type.FamilyName != null &&
                     type.FamilyName.Contains(familyNamePart);
          })
          .Select(e => new RebarElement(e, GetDimensionRefList(e, verticalRefNames), GetDimensionRefList(e, horizontalRefNames)))
          .ToList();


    public List<Reference> GetDimensionRefList(FamilyInstance elem, List<string> importantRefNameParts) {
        var allRefs = new List<Reference>();
        foreach(FamilyInstanceReferenceType referenceType in Enum.GetValues(typeof(FamilyInstanceReferenceType))) {
            allRefs.AddRange(elem.GetReferences(referenceType));
        }
        var refs = new List<Reference>();
        foreach(var reference in allRefs) {
            if(importantRefNameParts.Contains(elem.GetReferenceName(reference))) {
                refs.Add(reference);
            }
        }
        return refs;
    }
}
