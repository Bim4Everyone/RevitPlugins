using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;

using static RevitLintelsManager.Models.RevitElementRead;
using static RevitLintelsManager.Models.RevitGeometry;

namespace RevitLintelsManager.Services;

/// <summary>Разрешает цели навигации, выделяет их и приближает в собственном виде менеджера.</summary>
internal sealed class ManagerNavigationService(RevitRepository source, IManagerViewService views,
    SystemPluginConfig systemPluginConfig, ILocalizationService localization) : IManagerNavigationService {
    private Document Document => source.Document;

    /// <inheritdoc />
    public string EnsureView(UIApplication app, ManagerConfigSnapshot config) {
        try {
            return views.EnsureManagerView(app, config);
        } catch(Exception exception) {
            throw new InvalidOperationException(localization.GetLocalizedString("Manager.ViewPreparationFailed"),
                exception);
        }
    }

    /// <inheritdoc />
    public ManagerNavigationResult Navigate(UIApplication app, ManagerRequest request, ManagerSessionState state) {
        var elements = GetNavigationElements(app, request);
        if(elements is null) {
            return ManagerNavigationResult.SourceUnavailable;
        }
        if(elements.Count == 0) {
            return ManagerNavigationResult.TargetsUnavailable;
        }

        using var sectionBox = GetNavigationSectionBox(elements);
        if(sectionBox is null) {
            return ManagerNavigationResult.BoundsUnavailable;
        }

        string viewUniqueId = EnsureView(app, state.Config);
        return viewUniqueId is not null
            && ShowNavigationTargets(app, viewUniqueId, elements, sectionBox, request, state)
                ? ManagerNavigationResult.Completed : ManagerNavigationResult.SourceUnavailable;
    }

    /// <summary>Заново разрешает существующие цели по ключам исходного документа в ExternalEvent.</summary>
    private IReadOnlyList<Element> GetNavigationElements(UIApplication app, ManagerRequest request) {
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
    private BoundingBoxXYZ GetNavigationSectionBox(IReadOnlyList<Element> elements) {
        XYZ minimum = null;
        XYZ maximum = null;
        foreach(var element in elements) {
            using var bounds = ReadOptional(() => element.GetBoundingBox());
            if(bounds is null || !bounds.IsSet || !bounds.Enabled || !HasValidBounds(bounds.Min, bounds.Max)) {
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
    private bool ShowNavigationTargets(UIApplication app, string viewUniqueId, IReadOnlyList<Element> elements,
        BoundingBoxXYZ sectionBox, ManagerRequest request, ManagerSessionState state) {
        var uiDocument = GetNavigationDocument(app);
        if(uiDocument is null || !IsCurrentNavigation(request, state)) {
            return false;
        }

        var view = views.GetOwnedView(viewUniqueId, app.Application.Username);
        if(view is null) {
            throw new InvalidOperationException(localization.GetLocalizedString("Manager.NavigationViewUnavailable"));
        }
        if(elements.Any(element => !element.IsValidObject)) {
            throw new InvalidOperationException(localization.GetLocalizedString("Manager.NavigationTargetsChanged"));
        }
        views.SaveManagerSectionBox(view, sectionBox);

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
