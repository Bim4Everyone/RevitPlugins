using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.Intersections;
internal interface IClashFinder<TMep, TStructure> where TMep : Element where TStructure : Element {
    ICollection<ClashModel<TMep, TStructure>> FindClashes();
}
