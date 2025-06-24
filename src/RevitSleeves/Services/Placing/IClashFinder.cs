using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal interface IClashFinder<TMep, TStructure> where TMep : Element where TStructure : Element {
    ICollection<ClashModel<TMep, TStructure>> FindClashes();
}
