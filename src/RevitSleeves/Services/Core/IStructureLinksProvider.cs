using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Core;
internal interface IStructureLinksProvider {
    ICollection<RevitLinkType> GetLinks();
}
