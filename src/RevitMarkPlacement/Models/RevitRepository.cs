using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Comparators;

namespace RevitMarkPlacement.Models;

internal class RevitRepository : BaseViewModel {
    private readonly SystemPluginConfig _systemPluginConfig;

    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    public RevitRepository(UIApplication uiApplication, SystemPluginConfig systemPluginConfig) {
        UIApplication = uiApplication;
        _systemPluginConfig = systemPluginConfig;
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
    
    public Transaction StartTransaction(string transactionName) {
        return Document.StartTransaction(transactionName);
    }

    public IEnumerable<SpotDimensionType> GetSpotDimensionTypes(ISpotDimensionSelection selection) {
        return selection.GetElements()
            .Select(s => s.SpotDimensionType)
            .Distinct(new ElementIdComparer<SpotDimensionType>());
    }

    public AnnotationSymbol CreateAnnotationSymbol(FamilySymbol symbol, XYZ point, View view) {
        return (AnnotationSymbol) Document.Create.NewFamilyInstance(point, symbol, view);
    }

    public T GetElement<T>(ElementId elementId) where T : Element {
        return (T) Document.GetElement(elementId);
    }

    public IReadOnlyCollection<AnnotationSymbol> GetAnnotationSymbols() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(FamilyInstance))
            .OfType<AnnotationSymbol>()
            .Where(item => IsNeededAnnotationInstance(item))
            .ToArray();
    }

    public IReadOnlyCollection<AnnotationSymbolType> GetAnnotationSymbolTypes() {
        ICollection<string> familyNames = _systemPluginConfig.FamilyNames;
        ICollection<string> familyTypeNames = _systemPluginConfig.FamilyTypeNames;

        return new FilteredElementCollector(Document)
            .OfCategory(BuiltInCategory.OST_GenericAnnotation)
            .OfType<AnnotationSymbolType>()
            .Where(item =>
                familyNames.Any(f => f.Equals(item.Family.Name, StringComparison.CurrentCultureIgnoreCase))
                && familyTypeNames.Any(t => t.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase)))
            .ToArray();
    }

    public void MirrorAnnotation(FamilyInstance annotation, XYZ axis) {
        if(annotation.Location is LocationPoint point) {
            var plane = Plane.CreateByNormalAndOrigin(axis, point.Point);
            ElementTransformUtils.MirrorElement(Document, annotation.Id, plane);
            Document.Delete(annotation.Id);
        }
    }

    public void DeleteElement(Element element) {
        Document.Delete(element.Id);
    }

    private bool IsNeededAnnotationSymbol(FamilySymbol symbol, string typeName, string familyName) {
        return symbol.Name.Equals(typeName, StringComparison.CurrentCultureIgnoreCase)
               && symbol.Family.Name.Equals(familyName, StringComparison.CurrentCultureIgnoreCase);
    }

    private bool IsNeededAnnotationInstance(AnnotationSymbol instance) {
        var familyNames = _systemPluginConfig.FamilyNames;
        return instance.Symbol?.Family is not null
               && familyNames.Contains(instance.Symbol.Family.Name, StringComparer.CurrentCultureIgnoreCase);
    }
}
