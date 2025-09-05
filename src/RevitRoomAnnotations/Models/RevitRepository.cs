using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitRoomAnnotations.ViewModels;

namespace RevitRoomAnnotations.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// В случае если данный класс разрастается, рекомендуется его разделить на несколько.
/// </remarks>
internal class RevitRepository {
    private readonly ILocalizationService _localizationService;
    private readonly XYZ _defaultLocation = new(0, 0, 0);

    private const string _roomAnnotationName = "ТипАн_Помещение";
    private const string _viewsGroupParam = "_Группа Видов";
    private const string _viewsGroupValue = "02 АР-ЭОМ Задания входящие";
    private const string _projectStageParam = "_Стадия Проекта";
    private const string _projectStageValue = "Все стадии";
    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
        UIApplication = uiApplication;
        _localizationService = localizationService;
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

    public List<LinkedFile> GetLinkedFiles() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .Select(li => new LinkedFile(li, Document))
            .ToList();
    }

    public List<RevitRoom> GetRoomsFromLinks(IEnumerable<RevitLinkInstance> linkInstances) {
        var result = new List<RevitRoom>();
        foreach(var link in linkInstances) {
            var linkDoc = link.GetLinkDocument();
            if(linkDoc == null)
                continue;

            var rooms = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<SpatialElement>()
                .Select(r => new RevitRoom(r, link))
                .ToList();

            result.AddRange(rooms);
        }
        return result;
    }

    public IEnumerable<RevitAnnotation> GetAnnotations() {
        var view = CreateOrGetViewDrafting();

        return new FilteredElementCollector(Document, view.Id)
            .OfCategory(BuiltInCategory.OST_GenericAnnotation)
            .WhereElementIsNotElementType()
            .Cast<Element>()
            .Select(el => new RevitAnnotation(el));
    }

    public View CreateOrGetViewDrafting() {
        string roomEOM = _localizationService.GetLocalizedString("MainWindow.RoomEOM");
        string createEOMView = _localizationService.GetLocalizedString("MainWindow.CreateEomTransactionView");

        var view = new FilteredElementCollector(Document)
            .OfClass(typeof(ViewDrafting))
            .Cast<ViewDrafting>()
            .FirstOrDefault(v => v.Name == roomEOM);

        if(view != null) {
            return view;
        }

        var viewType = new FilteredElementCollector(Document)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.Drafting);

        
        using var t = Document.StartTransaction($"{createEOMView}");
        var newView = ViewDrafting.Create(Document, viewType.Id);
        newView.Name = roomEOM;

        newView.SetParamValue(_viewsGroupParam, _viewsGroupValue);
        newView.SetParamValue(_projectStageParam, _projectStageValue);

        t.Commit();
        return newView;
    }

    public FamilySymbol GetRoomAnnotationSymbol() {
        var symbol = new FilteredElementCollector(Document)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
            .FirstOrDefault(fs => fs.Name == _roomAnnotationName);

        return symbol is null ? null : symbol;
    }

    public RevitAnnotation CreateAnnotation(RevitRoom room) {
        var familySymbol = GetRoomAnnotationSymbol();
        var view = CreateOrGetViewDrafting();

        string createRoomAnnotation = _localizationService.GetLocalizedString("MainWindow.CreateRoomTransactionAnnotation");
        using var t = Document.StartTransaction($"{createRoomAnnotation}");
        var instance = Document.Create.NewFamilyInstance(_defaultLocation, familySymbol, view);
        UpdateAnnotation(instance, room);

        t.Commit();
        return new RevitAnnotation(instance);
    }

    public void UpdateAnnotation(Element annotation, RevitRoom room) {
        SetOrRemove(annotation, SharedParamsConfig.Instance.FopId.Name, room.RoomId.ToString());
        SetOrRemove(annotation, SharedParamsConfig.Instance.ApartmentNumberExtra.Name, room.AdditionalNumber);
        SetOrRemove(annotation, SharedParamsConfig.Instance.ApartmentNameExtra.Name, room.AdditionalName);
        SetOrRemove(annotation, SharedParamsConfig.Instance.RoomArea.Name, room.Area);
        SetOrRemove(annotation, SharedParamsConfig.Instance.RoomAreaWithRatio.Name, room.AreaWithCoefficient);
        SetOrRemove(annotation, SharedParamsConfig.Instance.ApartmentGroupName.Name, room.GroupSortOrder);
        SetOrRemove(annotation, SharedParamsConfig.Instance.RoomFireCategory.Name, room.RoomCategory);
        SetOrRemove(annotation, SharedParamsConfig.Instance.FireCompartmentShortName.Name, room.FireZone);
        SetOrRemove(annotation, SharedParamsConfig.Instance.Level.Name, room.Level);
        SetOrRemove(annotation, SharedParamsConfig.Instance.RoomGroupShortName.Name, room.Group);
        SetOrRemove(annotation, SharedParamsConfig.Instance.RoomBuildingShortName.Name, room.Building);
        SetOrRemove(annotation, SharedParamsConfig.Instance.RoomSectionShortName.Name, room.Section);
    }

    private void SetOrRemove(Element element, string paramName, string value) {
        if(string.IsNullOrEmpty(value)) {
            element.RemoveParamValue(paramName);
        } else {
            element.SetParamValue(paramName, value);
        }
    }

    private void SetOrRemove(Element element, string paramName, double? value) {
        if(value.HasValue) {
            element.SetParamValue(paramName, value.Value);
        } else {
            element.RemoveParamValue(paramName);
        }
    }

    public void DeleteElement(Element element) {
        string deleteRoomAnnotations = _localizationService.GetLocalizedString("MainWindow.DeleteRoomTransactionAnnotations");
        using var t = Document.StartTransaction($"{deleteRoomAnnotations}");
        Document.Delete(element.Id);
        t.Commit();
    }

    public IEnumerable<RevitAnnotation> GetAnnotationsNotOnTargetView(Document doc, View targetView) {
        return new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_GenericAnnotation)
            .WhereElementIsNotElementType()
            .Cast<FamilyInstance>()
            .Where(fi => fi.Symbol.Name == _roomAnnotationName && fi.OwnerViewId != targetView.Id)
            .Select(fi => new RevitAnnotation(fi));
    }
}
