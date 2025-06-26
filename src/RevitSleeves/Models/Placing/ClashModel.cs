using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

namespace RevitSleeves.Models.Placing;
internal class ClashModel<TMep, TStructure> where TMep : Element where TStructure : Element {
    private readonly RevitRepository _revitRepository;
    private readonly ClashModel _clashModel;

    public ClashModel(RevitRepository revitRepository, ClashModel clashModel) {
        _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _clashModel = clashModel ?? throw new System.ArgumentNullException(nameof(clashModel));
        var docInfos = _revitRepository.GetClashRevitRepository().DocInfos;

        MepElement = (TMep) _clashModel.MainElement.GetElement(docInfos);
        StructureElement = (TStructure) _clashModel.OtherElement.GetElement(docInfos);
    }

    public ClashModel(RevitRepository revitRepository, TMep mepElement, TStructure structureElement) {
        _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        MepElement = mepElement ?? throw new System.ArgumentNullException(nameof(mepElement));
        StructureElement = structureElement ?? throw new System.ArgumentNullException(nameof(structureElement));
    }


    public TMep MepElement { get; }

    public TStructure StructureElement { get; }
}
