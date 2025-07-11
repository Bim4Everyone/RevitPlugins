using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

using RevitClashDetective.Models.Interfaces;

using RevitSleeves.Models.Config;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;

using ParameterValueProvider = RevitClashDetective.Models.FilterableValueProviders.ParameterValueProvider;

namespace RevitSleeves.Models;

internal class RevitRepository {
    private readonly RevitClashDetective.Models.RevitRepository _clashRepository;
    private readonly IBimModelPartsService _modelPartsService;
    private readonly IView3DProvider _view3DProvider;
    private readonly View3D _sleeve3dView;

    public RevitRepository(UIApplication uiApplication,
        RevitClashDetective.Models.RevitRepository clashRepository,
        IBimModelPartsService modelPartsService,
        IView3DProvider view3DProvider) {

        UIApplication = uiApplication ?? throw new ArgumentNullException(nameof(uiApplication));
        _clashRepository = clashRepository ?? throw new ArgumentNullException(nameof(clashRepository));
        _modelPartsService = modelPartsService ?? throw new ArgumentNullException(nameof(modelPartsService));
        _view3DProvider = view3DProvider ?? throw new ArgumentNullException(nameof(view3DProvider));
        _sleeve3dView = _view3DProvider.GetView(
            Document, string.Format(NamesProvider.ViewNameSleeve, Application.Username));
    }

    public UIApplication UIApplication { get; }

    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    public Application Application => UIApplication.Application;

    public Document Document => ActiveUIDocument.Document;


    public View3D GetSleeve3dView() {
        return _sleeve3dView;
    }

    public IList<ParameterValueProvider> GetParameters(Document doc, Category category) {
        return _clashRepository.GetParameters(doc, [category]);
    }

    public RevitClashDetective.Models.RevitRepository GetClashRevitRepository() {
        return _clashRepository;
    }

    public Category GetCategory(BuiltInCategory category) {
        return Category.GetCategory(Document, category);
    }

    public ICollection<ElementId> GetLinkedElementIds<T>(RevitLinkInstance link) where T : Element {
        return new FilteredElementCollector(link.GetLinkDocument())
            .WhereElementIsNotElementType()
            .OfClass(typeof(T))
            .ToElementIds();
    }

    public ICollection<RevitLinkType> GetStructureLinkTypes() {
        return [.. new FilteredElementCollector(Document)
             .WhereElementIsElementType()
             .OfClass(typeof(RevitLinkType))
             .ToElements()
             .OfType<RevitLinkType>()
             .Where(l => _modelPartsService.InAnyBimModelParts(l, [BimModelPart.ARPart, BimModelPart.KRPart]))];
    }

    public ICollection<RevitLinkInstance> GetStructureLinkInstances() {
        return [.. new FilteredElementCollector(Document)
            .OfClass(typeof(RevitLinkInstance))
            .OfType<RevitLinkInstance>()
            .Where(link => _modelPartsService.InAnyBimModelParts(link, BimModelPart.ARPart, BimModelPart.KRPart)
                && RevitLinkType.IsLoaded(Document, link.GetTypeId()))];
    }

    public ICollection<SleeveModel> GetSleeves() {
        return [.. new FilteredElementCollector(Document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(FamilyInstance))
            .OfCategory(SleevePlacementSettingsConfig.SleeveCategory)
            .OfType<FamilyInstance>()
            .Where(f => f.Symbol.FamilyName.Equals(
                NamesProvider.FamilyNameSleeve,
                System.StringComparison.InvariantCultureIgnoreCase))
            .Select(f => new SleeveModel(f))];
    }

    public void DeleteElement(ElementId id) {
        Document.Delete(id);
    }

    public void DeleteElements(ICollection<ElementId> ids) {
        foreach(var id in ids) {
            DeleteElement(id);
        }
    }

    public FamilyInstance CreateInstance(FamilySymbol type, XYZ point, Level level) {
        if(type is null) {
            throw new ArgumentNullException(nameof(type));
        }

        if(point is null) {
            throw new ArgumentNullException(nameof(point));
        }

        if(level is null) {
            throw new ArgumentNullException(nameof(level));
        }

        if(!type.IsActive) {
            type.Activate();
        }

        point -= XYZ.BasisZ * level.ProjectElevation;
        var inst = Document.Create.NewFamilyInstance(point, type, level, StructuralType.NonStructural);
        Document.Regenerate(); // решение бага, когда значения параметров,
                               // которые назначались этому экземпляру сразу после создания, по итогу не назначались
        return inst;
    }

    public void RotateElement(Element element, XYZ point, Rotation angle) {
        if(point != null) {
            RotateElement(element, Line.CreateBound(point, new XYZ(point.X + 1, point.Y, point.Z)), angle.AngleOX);
            RotateElement(element, Line.CreateBound(point, new XYZ(point.X, point.Y + 1, point.Z)), angle.AngleOY);
            RotateElement(element, Line.CreateBound(point, new XYZ(point.X, point.Y, point.Z + 1)), angle.AngleOZ);
        }
    }

    public double ConvertFromInternal(double feetValue) {
        return UnitUtils.ConvertFromInternalUnits(feetValue, UnitTypeId.Millimeters);
    }

    public double ConvertToInternal(double mmValue) {
        return UnitUtils.ConvertToInternalUnits(mmValue, UnitTypeId.Millimeters);
    }

    private void RotateElement(Element element, Line axis, double angle) {
        if(Math.Abs(angle) > Application.AngleTolerance) {
            ElementTransformUtils.RotateElement(Document, element.Id, axis, angle);
        }
    }
}
