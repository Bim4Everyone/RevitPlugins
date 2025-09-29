using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.SimpleServices;


namespace RevitRoomAnnotations.Models;
internal class RevitRepository {
    public const string RoomAnnotationName = "ТипАн_Помещение";
    private const string _viewsGroupParam = "_Группа Видов";
    private const string _viewsGroupValue = "02 АР-ЭОМ Задания входящие";
    private const string _projectStageParam = "_Стадия Проекта";
    private const string _projectStageValue = "Все стадии";
    private readonly ILocalizationService _localizationService;

    public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
        UIApplication = uiApplication;
        _localizationService = localizationService;
    }

    public UIApplication UIApplication { get; }
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;


    // Метод получения списка LinkInstanceElement
    public List<LinkInstanceElement> GetLinkInstanceElements() {
        return new FilteredElementCollector(Document)
        .OfCategory(BuiltInCategory.OST_RvtLinks)
        .OfClass(typeof(RevitLinkInstance))
        .Cast<RevitLinkInstance>()
        .Select(linkInstance => new LinkInstanceElement(linkInstance, Document))
        .ToList();
    }

    // Метод получения списка RevitRoom из связанных файлов
    public List<RevitRoom> GetRevitRoomsFromLinks(IEnumerable<LinkInstanceElement> linkInstanceElements) {
        var result = new List<RevitRoom>();
        foreach(var linkInstanceElement in linkInstanceElements) {
            var linkDoc = linkInstanceElement.GetLinkDocument();
            if(linkDoc == null) {
                continue;
            }
            var rooms = new FilteredElementCollector(linkDoc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .WhereElementIsNotElementType()
                .Cast<SpatialElement>()
                .Select(room => new RevitRoom(room, linkInstanceElement, Document))
                .ToList();
            result.AddRange(rooms);
        }
        return result;
    }

    // Метод получения списка RevitAnnotation
    public IEnumerable<RevitAnnotation> GetRevitAnnotations(View targetView, bool searchOnTargetView) {
        return new FilteredElementCollector(Document, targetView.Id)
            .OfCategory(BuiltInCategory.OST_GenericAnnotation)
            .WhereElementIsNotElementType()
            .Cast<Element>()
            .Where(element => searchOnTargetView
                ? element.OwnerViewId == targetView.Id
                : element.OwnerViewId != targetView.Id)
            .Select(el => new RevitAnnotation(el) {
                CombinedID = el.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.FopId.Name)
            });
    }

    // Метод создания RevitAnnotation
    public RevitAnnotation CreateAnnotation(RevitRoom room, View view) {
        var familySymbol = GetRoomAnnotationSymbol();
        var instance = Document.Create.NewFamilyInstance(XYZ.Zero, familySymbol, view);
        UpdateAnnotation(instance, room);
        return new RevitAnnotation(instance) {
            CombinedID = room.CombinedId
        };
    }

    // Метод получения типоразмера семейства
    public FamilySymbol GetRoomAnnotationSymbol() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
            .FirstOrDefault(familySymbol => familySymbol.Name == RoomAnnotationName);
    }

    // Метод получения чертежного вида, существующего или создание нового
    public ViewDrafting GetViewDrafting() {
        string nameView = _localizationService.GetLocalizedString("RevitRepository.ViewName");
        var view = new FilteredElementCollector(Document)
            .OfClass(typeof(ViewDrafting))
            .Where(v => v.Name.Equals(nameView, StringComparison.OrdinalIgnoreCase))
            .Cast<ViewDrafting>()
            .FirstOrDefault();
        return view ?? CreateViewDrafting(nameView);
    }

    // Метод удаления элементов
    public void DeleteElement(Element element) {
        Document.Delete(element.Id);
    }

    // Метод обновления параметров RevitRoom
    public void UpdateAnnotation(Element annotation, RevitRoom room) {
        SetOrRemove(annotation, SharedParamsConfig.Instance.FopId.Name, room.CombinedId);
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

    // Метод создания нового чертежного вида
    private ViewDrafting CreateViewDrafting(string nameView) {
        var viewTypes = new FilteredElementCollector(Document)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>();
        var viewFamilyType = viewTypes
            .Where(vt => vt.ViewFamily == ViewFamily.Drafting)
            .First();
        var view = ViewDrafting.Create(Document, viewFamilyType.Id);
        view.Name = nameView;
        view.SetParamValue(_viewsGroupParam, _viewsGroupValue);
        view.SetParamValue(_projectStageParam, _projectStageValue);
        return view;
    }

    // Методы назначения или удаления параметров
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
}
