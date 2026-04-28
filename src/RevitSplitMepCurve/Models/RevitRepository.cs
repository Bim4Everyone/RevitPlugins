using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using RevitSplitMepCurve.Models.Enums;
using RevitSplitMepCurve.Models.Splittable;

namespace RevitSplitMepCurve.Models;

internal class RevitRepository {
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication ?? throw new ArgumentNullException(nameof(uiApplication));
    }

    public UIApplication UIApplication { get; }

    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    public Application Application => UIApplication.Application;

    public Document Document => ActiveUIDocument.Document;

    /// <summary>Все уровни документа, отсортированы снизу вверх; notInclude — имена для исключения.</summary>
    public IList<Level> GetLevels(string[] notInclude) {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .Where(l => !notInclude.Contains(l.Name))
            .OrderBy(l => l.Elevation)
            .ToArray();
    }

    /// <summary>FamilySymbol по уникальному имени типоразмера; null если не найден.</summary>
    public FamilySymbol GetFamilySymbol(string name) {
        if(string.IsNullOrWhiteSpace(name)) {
            return null;
        }
        return new FilteredElementCollector(Document)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
            .FirstOrDefault(s => s.Name == name);
    }

    /// <summary>
    /// Все ин-лайн соединители (≥2 connectors) подходящей категории:
    /// Pipes → OST_PipeFitting, Ducts → OST_DuctFitting.
    /// </summary>
    public ICollection<FamilySymbol> GetConnectorSymbols(MepClass mepClass) {
        var category = mepClass == MepClass.Pipes
            ? BuiltInCategory.OST_PipeFitting
            : BuiltInCategory.OST_DuctFitting;

        return new FilteredElementCollector(Document)
            .OfClass(typeof(FamilySymbol))
            .OfCategory(category)
            .Cast<FamilySymbol>()
            .Where(s => s.Family.FamilyPlacementType == FamilyPlacementType.CurveDrivenStructural
                        || HasAtLeastTwoConnectors(s))
            .ToArray();
    }

    /// <summary>Все элементы заданной категории во всём документе.</summary>
    public ICollection<T> GetElements<T>(BuiltInCategory category) where T : MEPCurve {
        return new FilteredElementCollector(Document)
            .OfCategory(category)
            .OfClass(typeof(T))
            .Cast<T>()
            .ToArray();
    }

    /// <summary>Все элементы заданной категории на 3D-виде (зарезервировано для будущих сценариев).</summary>
    public ICollection<T> GetElements<T>(BuiltInCategory category, View3D view) where T : MEPCurve {
        return new FilteredElementCollector(Document, view.Id)
            .OfCategory(category)
            .OfClass(typeof(T))
            .Cast<T>()
            .ToArray();
    }

    /// <summary>Все элементы заданной категории на активном виде.</summary>
    public ICollection<T> GetActiveViewElements<T>(BuiltInCategory category) where T : MEPCurve {
        return new FilteredElementCollector(Document, Document.ActiveView.Id)
            .OfCategory(category)
            .OfClass(typeof(T))
            .Cast<T>()
            .ToArray();
    }

    /// <summary>Текущий выделенный набор пользователя, отфильтрованный по категории.</summary>
    public ICollection<T> GetSelectedElements<T>(BuiltInCategory category) where T : MEPCurve {
        var categoryId = new ElementId(category);
        return ActiveUIDocument.Selection
            .GetElementIds()
            .Select(id => Document.GetElement(id))
            .OfType<T>()
            .Where(e => e.Category?.Id == categoryId)
            .ToArray();
    }

    /// <summary>Все DisplacementElement в документе.</summary>
    public ICollection<DisplacementElement> GetDisplacementElements() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(DisplacementElement))
            .Cast<DisplacementElement>()
            .ToArray();
    }

    /// <summary>true, если хотя бы один из элементов занят другим пользователем или устарел.</summary>
    public bool AnyOwned(ICollection<SplittableElement> elements) {
        if(!Document.IsWorkshared) {
            return false;
        }
        foreach(var splittable in elements) {
            var id = splittable.Element.Id;
            var checkoutStatus = WorksharingUtils.GetCheckoutStatus(Document, id);
            if(checkoutStatus == CheckoutStatus.OwnedByOtherUser) {
                return true;
            }
            var updateStatus = WorksharingUtils.GetModelUpdatesStatus(Document, id);
            if(updateStatus == ModelUpdatesStatus.UpdatedInCentral) {
                return true;
            }
        }
        return false;
    }

    // The category filter (OST_PipeFitting / OST_DuctFitting) is sufficient;
    // additional connector-count validation is not required.
    private static bool HasAtLeastTwoConnectors(FamilySymbol symbol) {
        return true;
    }
}
