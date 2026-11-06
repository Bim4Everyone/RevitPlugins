using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;
using dosymep.SimpleServices;

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
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
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

    /// <summary>
    /// Дает пользователю выбрать плиту.
    /// </summary>
    /// <returns>Id выбранной плиты.</returns>
    public ElementId PickSlab() {
        Reference reference = ActiveUIDocument.Selection.PickObject(
            ObjectType.Element, new SlabFilter(), _localizationService.GetLocalizedString("RevitUI.PickSlab"));

        return reference.ElementId;
    }

    /// <summary>
    /// Дает пользователю выбрать пилоны в текущем проекте.
    /// </summary>
    /// <returns>Id выбранных пилонов.</returns>
    public List<ElementId> PickPylonsFromModel() {
        IList<Reference> references = ActiveUIDocument.Selection.PickObjects(
            ObjectType.Element,
            new PylonFromModelFilter(),
            _localizationService.GetLocalizedString("RevitUI.PickPylons"));

        return references
            .Select(reference => reference.ElementId)
            .ToList();
    }

    /// <summary>
    /// Дает пользователю выбрать пилоны в связанных файлах.
    /// </summary>
    /// <returns>Ссылки на выбранные пилоны с указанием экземпляра связи.</returns>
    public List<LinkedPylonReference> PickPylonsFromLink() {
        IList<Reference> references = ActiveUIDocument.Selection.PickObjects(
            ObjectType.LinkedElement,
            new PylonFromLinkFilter(Document),
            _localizationService.GetLocalizedString("RevitUI.PickPylons"));

        return references
            .Select(reference => new LinkedPylonReference(reference.ElementId, reference.LinkedElementId))
            .ToList();
    }

    public Slab GetSlab(ElementId slabId) {
        Element slabElement = Document.GetElement(slabId);

        return new Slab(slabElement);
    }

    /// <summary>
    /// Возвращает пилоны, выбранные в текущем проекте и в связанных файлах.
    /// </summary>
    /// <param name="modelPylonIds">Id пилонов в текущем проекте.</param>
    /// <param name="linkedPylons">Ссылки на пилоны в связанных файлах.</param>
    public List<IPylon> GetPylons(
        IEnumerable<ElementId> modelPylonIds, IEnumerable<LinkedPylonReference> linkedPylons) {
        List<IPylon> pylons = modelPylonIds
            .Select(id => (IPylon) new StructColumnPylon(Document.GetElement(id)))
            .ToList();

        foreach(LinkedPylonReference linkedPylon in linkedPylons) {
            RevitLinkInstance linkInstance = (RevitLinkInstance) Document.GetElement(linkedPylon.LinkInstanceId);
            Document linkedDoc = linkInstance.GetLinkDocument();

            pylons.Add(new StructColumnPylon(linkedDoc.GetElement(linkedPylon.PylonId)));
        }

        return pylons;
    }

    /// <summary>
    /// Возвращает тип каркаса по имени семейства и типа, или null, если такого типа нет в проекте.
    /// </summary>
    public FamilySymbol GetFrameSymbol(string familyName, string typeName) {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>()
            .FirstOrDefault(symbol => symbol.FamilyName == familyName && symbol.Name == typeName);
    }

    /// <summary>
    /// Проверяет, каких параметров не хватает экземпляру типа каркаса.
    /// </summary>
    /// <remarks>
    /// Создает пробный экземпляр во временной транзакции и откатывает ее, поэтому учитываются
    /// и параметры семейства, и параметры проекта, назначенные категории семейства.
    /// </remarks>
    /// <param name="symbol">Тип каркаса.</param>
    /// <param name="paramNames">Имена обязательных параметров.</param>
    /// <returns>Имена отсутствующих параметров.</returns>
    public List<string> GetMissingFrameParams(FamilySymbol symbol, IEnumerable<string> paramNames) {
        string transactionName = _localizationService.GetLocalizedString("RevitUI.CheckFrameTypeTransaction");
        using Transaction transaction = Document.StartTransaction(transactionName);

        if(!symbol.IsActive) {
            symbol.Activate();
        }

        FamilyInstance instance = Document.Create.NewFamilyInstance(XYZ.Zero, symbol, StructuralType.NonStructural);
        List<string> missingParams = paramNames
            .Where(paramName => instance.LookupParameter(paramName) == null)
            .ToList();

        transaction.RollBack();
        return missingParams;
    }

    public Frame CreateFrame(FamilySymbol frameSymbol, IFrameParams frameParams, XYZ location) {
        FamilyInstance frameInstance = Document.Create.NewFamilyInstance(
            location, frameSymbol, StructuralType.NonStructural);

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
