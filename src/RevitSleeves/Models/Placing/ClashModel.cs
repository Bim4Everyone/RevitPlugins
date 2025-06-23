using Autodesk.Revit.DB;

namespace RevitSleeves.Models.Placing;
internal class ClashModel<TMep, TStructure> where TMep : Element where TStructure : Element {
    public ClashModel() {

    }


    public TMep MepElement { get; }

    public TStructure StructureElement { get; }
}
