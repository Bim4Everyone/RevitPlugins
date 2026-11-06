using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using RevitPunchingRebar.Models.Interfaces;
using RevitPunchingRebar.Models.SelectionFilters;

using Application = Autodesk.Revit.ApplicationServices.Application;

namespace RevitPunchingRebar.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// В случае если данный класс разрастается, рекомендуется его разделить на несколько.
/// </remarks>
internal class RevitRepository {
    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
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

    /// <summary>
    /// Дает пользователю выбрать плиту.
    /// </summary>
    /// <returns>UniqueId выбранной плиты.</returns>
    public string PickSlab() {
        Reference reference = ActiveUIDocument.Selection.PickObject(
            ObjectType.Element, new SlabFilter(), "Выберите плиту");

        return Document.GetElement(reference.ElementId).UniqueId;
    }

    /// <summary>
    /// Дает пользователю выбрать пилоны в текущем проекте.
    /// </summary>
    /// <returns>UniqueId выбранных пилонов.</returns>
    public List<string> PickPylonsFromModel() {
        IList<Reference> references = ActiveUIDocument.Selection.PickObjects(
            ObjectType.Element, new PylonFromModelFilter(), "Выберите пилоны");

        return references
            .Select(reference => Document.GetElement(reference).UniqueId)
            .ToList();
    }

    /// <summary>
    /// Дает пользователю выбрать пилоны в связанных файлах.
    /// </summary>
    /// <returns>Словарь: UniqueId пилона в связанном файле - название связанного файла.</returns>
    public Dictionary<string, string> PickPylonsFromLink() {
        IList<Reference> references = ActiveUIDocument.Selection.PickObjects(
            ObjectType.LinkedElement, new PylonFromLinkFilter(Document), "Выберите пилоны");

        Dictionary<string, string> selectedPylons = new Dictionary<string, string>();
        foreach(Reference reference in references) {
            RevitLinkInstance link = (RevitLinkInstance) Document.GetElement(reference.ElementId);
            Document linkedDoc = link.GetLinkDocument();

            selectedPylons.Add(linkedDoc.GetElement(reference.LinkedElementId).UniqueId, linkedDoc.Title);
        }

        return selectedPylons;
    }

    public Slab GetSlab(string slabUniqId) {
        Element slabElement = Document.GetElement(slabUniqId);

        return new Slab(slabElement);
    }

    public List<T> GetPylons<T>(RevitSettings settings, Func<Element, T> factory) where T : IPylon {
        List<T> pylons = new List<T>();

        if(settings.PylonsFromModel.Count != 0) {
            List<string> ids = settings.PylonsFromModel;

            foreach(string uniqId in ids) {
                Element element = Document.GetElement(uniqId);
                pylons.Add(factory(element));
            }
        } else {
            Dictionary<string, string> settingPylons = settings.PylonsFromLink;

            foreach(var pylon in settingPylons) {
                string uniqId = pylon.Key;
                string linkedDocTitle = pylon.Value;

                RevitLinkInstance linkInstance = new FilteredElementCollector(Document)
                    .OfClass(typeof(RevitLinkInstance))
                    .Cast<RevitLinkInstance>()
                    .FirstOrDefault(link => {
                        return link.GetLinkDocument().Title == linkedDocTitle;
                    });

                Document linkedDoc = linkInstance.GetLinkDocument();
                Element element = linkedDoc.GetElement(uniqId);
                pylons.Add(factory(element));
            }
        }

        return pylons;
    }

    public Frame CreateFrame(IFrameParams frameParams, XYZ location) {
        FilteredElementCollector collector = new FilteredElementCollector(Document).OfClass(typeof(FamilySymbol));
        FamilySymbol frameFS = collector.ToElements().Cast<FamilySymbol>()
                                            .Where(x => x.get_Parameter(BuiltInParameter.SYMBOL_FAMILY_NAME_PARAM).AsString() == frameParams.FrameFamilyName)
                                            .Where(x => x.get_Parameter(BuiltInParameter.SYMBOL_NAME_PARAM).AsString() == frameParams.FrameFamilyTypeName)
                                            .FirstOrDefault();

        FamilyInstance frameInstance = Document.Create.NewFamilyInstance(location, frameFS, StructuralType.NonStructural);

        Frame frame = new Frame(frameInstance, frameParams);

        return frame;
    }

    public IList<string> GetFamilyNames() {
        IList<string> familyNames = new FilteredElementCollector(Document)
        .OfClass(typeof(Family))
        .Cast<Family>()
        .Select(f => f.Name)
        .OrderBy(n => n)
        .ToList();

        return familyNames;
    }

    public IList<string> GetFamilyTypes(string familyName) {
        IList<string> familyTypes = new FilteredElementCollector(Document)
        .OfClass(typeof(FamilySymbol))
        .Cast<FamilySymbol>()
        .Where(fs => fs.Family.Name.Equals(familyName)).
        Select(f => f.Name)
        .OrderBy(n => n)
        .ToList();

        return familyTypes;
    }
}
