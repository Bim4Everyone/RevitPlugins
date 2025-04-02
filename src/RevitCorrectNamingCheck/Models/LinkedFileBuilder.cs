using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using dosymep.Bim4Everyone;
using RevitCorrectNamingCheck.Helpers;

namespace RevitCorrectNamingCheck.Models
{
    internal class LinkedFileBuilder {
        public LinkedFile Build(ElementId id, string name, WorksetInfo typeWorkset, WorksetInfo instanceWorkset,List<BimModelPart> bimModelParts) {
            var bimIdentifiers = bimModelParts
                .SelectMany(p => new[] { p.Id, p.Name })
                .Distinct()
                .ToList();

            var fileNameStatus = GetFileNameStatus(name, bimIdentifiers);
            var file = new LinkedFile(id, name, typeWorkset, instanceWorkset) {
                FileNameStatus = fileNameStatus
            };

            if(fileNameStatus == NameStatus.Correct) {
                var currentPart = GetCurrentBimModelPart(name, bimModelParts);

                SetWorksetInfo(typeWorkset, currentPart, bimIdentifiers);
                SetWorksetInfo(instanceWorkset, currentPart, bimIdentifiers);
            }

            return file;
        }

        private void SetWorksetInfo(WorksetInfo workset, BimModelPart currentPart, List<string> bimIdentifiers) {
            workset.WorksetNameStatus = GetWorksetNameStatus(workset.Name, currentPart, bimIdentifiers);
        }

        private NameStatus GetFileNameStatus(string name, List<string> bimIdentifiers) {
            int matches = bimIdentifiers.Count(part => NamingRulesHelper.ContainsPart(name, part));
            return matches == 1 ? NameStatus.Correct : NameStatus.Incorrect;
        }

        private BimModelPart GetCurrentBimModelPart(string name, List<BimModelPart> allParts) {
            var matches = allParts
                .Where(p => NamingRulesHelper.ContainsPart(name, p.Name) || NamingRulesHelper.ContainsPart(name, p.Id))
                .ToList();

            return matches.Count == 1 ? matches[0] : null;
        }

        private NameStatus GetWorksetNameStatus(string worksetName, BimModelPart currentPart, List<string> identifiers) {
            int matchCount = identifiers.Count(part => NamingRulesHelper.ContainsPart(worksetName, part));
            bool isCurrent = NamingRulesHelper.MatchesCurrentPart(worksetName, currentPart);
            bool isLink = NamingRulesHelper.IsLinkWorkset(worksetName);

            if(matchCount == 0) {
                return NameStatus.None;
            }
                
            if(matchCount == 1) {
                if(isCurrent) {
                    return isLink ? NameStatus.Correct : NameStatus.PartialCorrect;
                }
                return NameStatus.None;
            }

            return NameStatus.Incorrect;
        }
    }
}
