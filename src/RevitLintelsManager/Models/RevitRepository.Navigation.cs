using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitLintelsManager.Models;

internal partial class RevitRepository {
    internal string ManagerSectionBoxTransactionName =>
        localization.GetLocalizedString("Manager.NavigationSectionBoxTransaction");

    /// <summary>Заново разрешает существующие цели по ключам исходного документа в ExternalEvent.</summary>
    public IReadOnlyList<Element> GetNavigationElements(UIApplication app, ManagerRequest request) {
        if(GetNavigationDocument(app) is null) {
            return null;
        }

        return request.NavigationItems.Where(key => key.DocumentToken == request.DocumentToken)
            .SelectMany(key => new[] { key.OpeningUniqueId, key.LintelUniqueId })
            .Where(key => !string.IsNullOrEmpty(key))
            .Distinct(StringComparer.Ordinal)
            .Select(key => ReadOptional(() => Document.GetElement(key)))
            .Where(element => element?.IsValidObject == true)
            .ToList().AsReadOnly();
    }

    /// <summary>Возвращает общий мировой SectionBox или null при недоступных границах хотя бы одной цели.</summary>
    public BoundingBoxXYZ GetNavigationSectionBox(IReadOnlyList<Element> elements) {
        XYZ minimum = null;
        XYZ maximum = null;
        foreach(var element in elements) {
            using var bounds = ReadOptional(() => element.GetBoundingBox());
            if(bounds is null || !bounds.IsSet || !bounds.Enabled || !HasNavigationBounds(bounds.Min, bounds.Max)) {
                return null;
            }

            foreach(var corner in GetWorldCorners(bounds)) {
                if(!IsFinitePoint(corner)) {
                    return null;
                }
                minimum = minimum is null ? corner
                    : new XYZ(Math.Min(minimum.X, corner.X), Math.Min(minimum.Y, corner.Y),
                        Math.Min(minimum.Z, corner.Z));
                maximum = maximum is null ? corner
                    : new XYZ(Math.Max(maximum.X, corner.X), Math.Max(maximum.Y, corner.Y),
                        Math.Max(maximum.Z, corner.Z));
            }
        }
        if(minimum is null || maximum is null) {
            return null;
        }

        double margin = UnitUtils.ConvertToInternalUnits(systemPluginConfig.NavigationSectionBoxMarginMm,
            UnitTypeId.Millimeters);
        var padding = new XYZ(margin, margin, margin);
        minimum -= padding;
        maximum += padding;
        if(!IsFinitePoint(minimum) || !IsFinitePoint(maximum)
            || minimum.X >= maximum.X || minimum.Y >= maximum.Y || minimum.Z >= maximum.Z) {
            return null;
        }
        return new BoundingBoxXYZ { Min = minimum, Max = maximum, Enabled = true };
    }

    /// <summary>Сохраняет рамку своего вида, затем активирует вид, выделяет цели и приближает их.</summary>
    public bool ShowNavigationTargets(UIApplication app, string viewUniqueId, IReadOnlyList<Element> elements,
        BoundingBoxXYZ sectionBox, ManagerRequest request, ManagerSessionState state) {
        var uiDocument = GetNavigationDocument(app);
        if(uiDocument is null || !IsCurrentNavigation(request, state)) {
            return false;
        }

        var schema = Schema.Lookup(_managerViewSchemaId);
        if(Document.GetElement(viewUniqueId) is not View3D view || schema is null
            || !IsManagerView(view, schema, app.Application.Username)) {
            throw new InvalidOperationException(localization.GetLocalizedString("Manager.NavigationViewUnavailable"));
        }
        if(elements.Any(element => !element.IsValidObject)) {
            throw new InvalidOperationException(localization.GetLocalizedString("Manager.NavigationTargetsChanged"));
        }
        SaveManagerSectionBox(view, sectionBox);

        uiDocument = GetNavigationDocument(app);
        if(uiDocument is null || !IsCurrentNavigation(request, state) || !view.IsValidObject) {
            return false;
        }
        if(uiDocument.ActiveView?.Id != view.Id) {
            uiDocument.ActiveView = view;
        }
        uiDocument = GetNavigationDocument(app);
        if(uiDocument is null || !IsCurrentNavigation(request, state) || !view.IsValidObject
            || uiDocument.ActiveView?.Id != view.Id) {
            return false;
        }

        // Переключение вида может само изменить выделение. Учитываем его и при отказе SetElementIds.
        SetNavigationStamp(request, state, viewUniqueId, uiDocument.Selection.GetElementIds());
        var elementIds = elements.Where(element => element.IsValidObject).Select(element => element.Id).ToList();
        if(elementIds.Count != elements.Count) {
            throw new InvalidOperationException(localization.GetLocalizedString("Manager.NavigationTargetsChanged"));
        }
        uiDocument.Selection.SetElementIds(elementIds);
        uiDocument = GetNavigationDocument(app);
        if(uiDocument is null || !IsCurrentNavigation(request, state) || !view.IsValidObject
            || uiDocument.ActiveView?.Id != view.Id) {
            return false;
        }

        var actualSelection = uiDocument.Selection.GetElementIds();
        SetNavigationStamp(request, state, viewUniqueId, actualSelection);
        ZoomToNavigationBounds(uiDocument, view, sectionBox);
        if(!new HashSet<ElementId>(elementIds).SetEquals(actualSelection)) {
            throw new InvalidOperationException(localization.GetLocalizedString("Manager.NavigationSelectionFailed"));
        }
        return true;
    }

    // Проверяет исходный проект до разрешения ключей или изменения вида.
    private UIDocument GetNavigationDocument(UIApplication app) {
        var uiDocument = app.ActiveUIDocument;
        var activeDocument = uiDocument?.Document;
        if(!Document.IsValidObject || activeDocument?.IsValidObject != true || !Document.Equals(activeDocument)) {
            return null;
        }
        if(Document.IsReadOnly || Document.IsReadOnlyFile || Document.IsModifiable || Document.IsFamilyDocument) {
            throw new InvalidOperationException(
                localization.GetLocalizedString("Manager.NavigationSourceUnavailable"));
        }
        return uiDocument;
    }

    // Не продолжает запрос после закрытия, перехода в другой документ или изменения поколения.
    private static bool IsCurrentNavigation(ManagerRequest request, ManagerSessionState state) {
        return !state.IsClosing && !state.IsBlocked && request.DocumentToken == state.DocumentToken
            && request.Generation == state.Generation;
    }

    // Проверяет локальные границы, включая случай плоской геометрии.
    private static bool HasNavigationBounds(XYZ minimum, XYZ maximum) {
        return IsFinitePoint(minimum) && IsFinitePoint(maximum)
            && minimum.X <= maximum.X && minimum.Y <= maximum.Y && minimum.Z <= maximum.Z
            && (minimum.X < maximum.X || minimum.Y < maximum.Y || minimum.Z < maximum.Z);
    }

    // Проверяет числовые координаты перед формированием рамки.
    private static bool IsFinitePoint(XYZ point) {
        return point is not null && IsFiniteCoordinate(point.X) && IsFiniteCoordinate(point.Y)
            && IsFiniteCoordinate(point.Z);
    }

    // Исключает значения, которые не представляют координату модели.
    private static bool IsFiniteCoordinate(double value) {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    // Преобразует все восемь углов, поскольку две диагональные точки могут поменять порядок после поворота.
    private static IEnumerable<XYZ> GetWorldCorners(BoundingBoxXYZ bounds) {
        var minimum = bounds.Min;
        var maximum = bounds.Max;
        var transform = bounds.Transform;
        foreach(double x in new[] { minimum.X, maximum.X }) {
            foreach(double y in new[] { minimum.Y, maximum.Y }) {
                foreach(double z in new[] { minimum.Z, maximum.Z }) {
                    yield return transform.OfPoint(new XYZ(x, y, z));
                }
            }
        }
    }

    // Записывает только рамку служебного вида в отдельной короткой транзакции.
    private void SaveManagerSectionBox(View3D view, BoundingBoxXYZ sectionBox) {
        SaveManagerViewChanges(ManagerSectionBoxTransactionName, () => {
            view.SetSectionBox(sectionBox);
            view.IsSectionBoxActive = true;
            if(!view.IsSectionBoxActive) {
                throw new InvalidOperationException(
                    localization.GetLocalizedString("Manager.NavigationSectionBoxFailed"));
            }
        });
    }

    // Передаёт Idling ожидаемый вид и независимые идентификаторы программного выделения.
    private static void SetNavigationStamp(ManagerRequest request, ManagerSessionState state, string viewUniqueId,
        IEnumerable<ElementId> selection) {
        state.NavigationStamp = new ManagerNavigationStamp(request.DocumentToken, request.Generation, viewUniqueId,
            selection.Select(id => (long) id.GetIdValue()));
    }

    // Проецирует мировую рамку на экранную плоскость текущей камеры и приближает её без изменения ориентации.
    private void ZoomToNavigationBounds(UIDocument uiDocument, View3D view, BoundingBoxXYZ sectionBox) {
        var openViews = uiDocument.GetOpenUIViews();
        try {
            var uiView = openViews.FirstOrDefault(candidate => candidate.ViewId == view.Id);
            if(uiView is null) {
                throw new InvalidOperationException(
                    localization.GetLocalizedString("Manager.NavigationViewportUnavailable"));
            }
            var halfSize = (sectionBox.Max - sectionBox.Min) * 0.5;
            var center = sectionBox.Min + halfSize;
            var right = view.RightDirection;
            var up = view.UpDirection;
            var screenOffset = right * GetProjectedHalfSize(halfSize, right)
                + up * GetProjectedHalfSize(halfSize, up);
            uiView.ZoomAndCenterRectangle(center - screenOffset, center + screenOffset);
        } finally {
            foreach(var uiView in openViews) {
                uiView.Dispose();
            }
        }
    }

    // Вычисляет половину размера мировой рамки вдоль одной оси экрана.
    private static double GetProjectedHalfSize(XYZ halfSize, XYZ direction) {
        return halfSize.X * Math.Abs(direction.X) + halfSize.Y * Math.Abs(direction.Y)
            + halfSize.Z * Math.Abs(direction.Z);
    }
}
