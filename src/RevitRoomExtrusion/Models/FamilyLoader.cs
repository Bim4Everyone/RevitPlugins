using System;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using dosymep.Revit;
using dosymep.SimpleServices;


namespace RevitRoomExtrusion.Models;
internal class FamilyLoader {
    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private readonly FamilyLoadOptions _familyLoadOptions;

    public FamilyLoader(
        ILocalizationService localizationService,
        RevitRepository revitRepository,
        FamilyLoadOptions familyLoadOptions) {
        _revitRepository = revitRepository;
        _familyLoadOptions = familyLoadOptions;
        _localizationService = localizationService;
    }

    public FamilySymbol LoadFamilyInstance(string path) {
        Family family = null;
        FamilySymbol familySymbol = null;

        string transactionNameLoad = _localizationService.GetLocalizedString("FamilyLoader.TransactionNameLoad");
        using(var t = _revitRepository.Document.StartTransaction(transactionNameLoad)) {
            _revitRepository.Document.LoadFamily(path, _familyLoadOptions, out family);

            familySymbol = _revitRepository.GetFamilySymbol(family);

            if(familySymbol != null) {
                if(!familySymbol.IsActive) {
                    familySymbol.Activate();
                }
            }
            t.Commit();
        }
        DeleteRoomFamily(path);
        return familySymbol;
    }

    private void DeleteRoomFamily(string famPath) {
        try {
            if(File.Exists(famPath)) {
                File.Delete(famPath);
            }
        } catch(UnauthorizedAccessException) {
        }
    }

    public void PlaceFamilyInstance(FamilySymbol symbol, IGrouping<int, RoomElement> groupRooms, double location) {
        var firstRoom = groupRooms
            .FirstOrDefault();

        double locationRoom = 0;
        if(firstRoom != null) {
            locationRoom = firstRoom.LocationRoom;
        }

        double locationFt = UnitUtils.ConvertToInternalUnits(location, UnitTypeId.Millimeters);
        double locationPlace = locationFt - locationRoom + _revitRepository.GetBasePointLocation();
        string transactionNamePlace = _localizationService.GetLocalizedString("FamilyLoader.TransactionNamePlace");
        using var t = _revitRepository.Document.StartTransaction(transactionNamePlace);
        var xyz = new XYZ(0, 0, locationPlace);
        var familyInstance = _revitRepository.Document.Create.NewFamilyInstance(
            xyz,
            symbol,
            StructuralType.NonStructural);
        t.Commit();
    }

    public bool IsFamilyInstancePlaced(string familyName) {
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
