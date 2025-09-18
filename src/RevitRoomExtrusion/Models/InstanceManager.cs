using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;

using dosymep.Revit;
using dosymep.SimpleServices;


namespace RevitRoomExtrusion.Models;
internal class InstanceManager {
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private readonly FamilyLoadOptions _familyLoadOptions;

    public InstanceManager(
        ILocalizationService localizationService, RevitRepository revitRepository, FamilyLoadOptions familyLoadOptions) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _familyLoadOptions = familyLoadOptions;
    }

    // Основной метод по созданию, загрузке семейства и размещению в модели
    public void CreateInstances(List<Room> listRoom, string familyName, double extrusionHeight, bool joinExtrusionChecked) {
        string transactionName = _localizationService.GetLocalizedString("InstanceManager.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);
        var view3d = _revitRepository.GetView3D(familyName);
        var roomElements = listRoom
            .Select(room => new RoomElement(_revitRepository, room, view3d))
            .ToList();
        var groupedRooms = roomElements
            .GroupBy(re => re.GetCleanFloorLocationPoint());
        foreach(var groupRooms in groupedRooms) {
            int locationKey = groupRooms.Key;
            var tempFamilyDocument = new TempFamilyDocument(
                _localizationService, _revitRepository, _familyLoadOptions, familyName, locationKey);

            var famSymbol = tempFamilyDocument.GetFamilySymbol(groupRooms.ToList(), extrusionHeight, joinExtrusionChecked);

            if(!IsPlacedFamilyInstance(familyName)) {
                PlaceFamilyInstance(famSymbol, groupRooms, locationKey);
            }
        }
        t.Commit();
    }

    // Метод по размещению экземпляров семейства в модели
    public void PlaceFamilyInstance(FamilySymbol symbol, IGrouping<int, RoomElement> groupRooms, double location) {
        var firstRoom = groupRooms
            .FirstOrDefault();
        double locationRoom = 0;
        if(firstRoom != null) {
            locationRoom = firstRoom.GetOriginalLocationPoint();
        }
        double locationFt = UnitUtils.ConvertToInternalUnits(location, UnitTypeId.Millimeters);
        double locationPlace = locationFt - locationRoom + _revitRepository.GetBasePointLocation();
        var xyz = new XYZ(0, 0, locationPlace);
        var familyInstance = _revitRepository.Document.Create.NewFamilyInstance(
            xyz,
            symbol,
            StructuralType.NonStructural);
    }

    // Метод проверки размещенных семейств
    public bool IsPlacedFamilyInstance(string familyName) {
        var family = new FilteredElementCollector(_revitRepository.Document)
            .OfClass(typeof(Family))
            .FirstOrDefault(f => f.Name.Equals(familyName, StringComparison.InvariantCultureIgnoreCase));
        if(family != null) {
            var symbolId = new FilteredElementCollector(_revitRepository.Document)
                .WherePasses(new FamilySymbolFilter(family.Id))
                .Select(selector => selector.Id)
                .FirstOrDefault();
            if(symbolId.IsNotNull()) {
                return new FilteredElementCollector(_revitRepository.Document)
                    .WherePasses(new FamilyInstanceFilter(_revitRepository.Document, symbolId))
                    .Any();
            }
        }
        return false;
    }
}
