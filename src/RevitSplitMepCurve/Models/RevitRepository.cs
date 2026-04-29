using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

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

    public ICollection<Level> GetLevels(params string[] notIncludeNames) {
        return new FilteredElementCollector(Document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(Level))
            .OfType<Level>()
            .Where(l => !notIncludeNames.Contains(l.Name, StringComparer.CurrentCultureIgnoreCase))
            .ToArray();
    }

    public ICollection<FamilySymbol> GetConnectorSymbols(BuiltInCategory category) {
        return new FilteredElementCollector(Document)
            .WhereElementIsElementType()
            .OfClass(typeof(FamilySymbol))
            .OfCategory(category)
            .OfType<FamilySymbol>()
            .ToArray();
    }

    /// <summary>Все элементы заданной категории во всём документе.</summary>
    public ICollection<T> GetElements<T>(BuiltInCategory category) where T : MEPCurve {
        return new FilteredElementCollector(Document)
            .WhereElementIsNotElementType()
            .OfCategory(category)
            .OfClass(typeof(T))
            .OfType<T>()
            .ToArray();
    }

    /// <summary>Все элементы заданной категории на активном виде.</summary>
    public ICollection<T> GetActiveViewElements<T>(BuiltInCategory category) where T : MEPCurve {
        return new FilteredElementCollector(Document, Document.ActiveView.Id)
            .WhereElementIsNotElementType()
            .OfCategory(category)
            .OfClass(typeof(T))
            .OfType<T>()
            .ToArray();
    }

    /// <summary>Текущий выделенный набор пользователя, отфильтрованный по категории.</summary>
    public ICollection<T> GetSelectedElements<T>(BuiltInCategory category) where T : MEPCurve {
        return new FilteredElementCollector(Document, ActiveUIDocument.Selection.GetElementIds())
            .WhereElementIsNotElementType()
            .OfCategory(category)
            .OfClass(typeof(T))
            .OfType<T>()
            .ToArray();
    }

    /// <summary>true, если у пользователя выбран хотя бы один элемент.</summary>
    public bool HasSelectedElements() {
        return ActiveUIDocument.Selection.GetElementIds().Count > 0;
    }

    /// <summary>Базовая точка проекта.</summary>
    public BasePoint GetProjectBasePoint() {
        return BasePoint.GetProjectBasePoint(Document);
    }

    /// <summary>Группы уровней с одинаковыми отметками</summary>
    public ICollection<ICollection<Level>> GetDuplicateLevels() {
        var levels = GetLevels([]);
        return GroupIntersectingLevels(levels);
    }

    public ICollection<DisplacementElement> GetDisplacementElements() {
        return new FilteredElementCollector(Document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(DisplacementElement))
            .OfType<DisplacementElement>()
            .ToArray();
    }

    public bool IsSyncRequired(ICollection<SplittableElement> elements) {
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

    public void ShowElements(params Element[] elements) {
        if(elements is null
           || elements.Length == 0) {
            return;
        }

        ElementId[] elementIds = elements
            .Select(item => item.Id)
            .ToArray();

        ActiveUIDocument.ShowElements(elementIds);
        ActiveUIDocument.Selection.SetElementIds(elementIds);
    }

    /// <summary>
    /// Группирует уровни, которые пересекаются по высоте с учетом допуска ревита по длине линий
    /// </summary>
    private ICollection<ICollection<Level>> GroupIntersectingLevels(ICollection<Level> levels) {
        if(levels == null
           || !levels.Any()) {
            return [];
        }

        double tolerance = Application.ShortCurveTolerance;

        var sortedLevels = levels.OrderBy(l => l.Elevation).ToArray();
        var groups = new List<ICollection<Level>>();
        var currentGroup = new List<Level> { sortedLevels[0] };

        for(int i = 1; i < sortedLevels.Length; i++) {
            var prevLevel = sortedLevels[i - 1];
            var currentLevel = sortedLevels[i];

            double prevTop = prevLevel.Elevation + tolerance;
            double currentBottom = currentLevel.Elevation - tolerance;

            if(currentBottom <= prevTop) {
                currentGroup.Add(currentLevel);
            } else {
                groups.Add(currentGroup);
                currentGroup = [currentLevel];
            }
        }

        groups.Add(currentGroup);

        return groups;
    }
}
