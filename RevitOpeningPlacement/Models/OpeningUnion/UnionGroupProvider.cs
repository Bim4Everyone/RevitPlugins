using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningUnion {
    internal class UnionGroupProvider {
        private readonly IIntersectionProvider _intersectionProvider;

        public UnionGroupProvider(IIntersectionProvider intersectionProvider) {
            _intersectionProvider = intersectionProvider;
        }

        public List<OpeningsGroup> GetOpeningGroups(IList<FamilyInstance> elements, IProgress<int> progress, CancellationToken ct) {
            List<OpeningsGroup> groups = new List<OpeningsGroup>();
            int count = 0;
            var elementsWrappers = elements.Select(item => new FamilyInstanceWrapper() { Element = item }).ToList();
            for(int i = 0; i < elementsWrappers.Count; i++) {
                var group = new OpeningsGroup();
                group.AddElement(elementsWrappers[i].Element);
                elementsWrappers.RemoveAt(i);
                i--;

                progress.Report(count++);
                ct.ThrowIfCancellationRequested();
                
                GetIntersectedElements(elementsWrappers, group);

                progress.Report(count += group.Elements.Count - 1);

                if(group.Elements.Count > 1) {
                    groups.Add(group);
                }
            }
            return groups;
        }

        private void GetIntersectedElements(IList<FamilyInstanceWrapper> elements, OpeningsGroup group) {
            var intersections = _intersectionProvider.GetIntersectedElements(group, elements);
            if(intersections.Count > 0) {
                group.AddRangeElements(intersections.Select(item=>item.Element).ToArray());
                foreach(var intersection in intersections) {
                    elements.Remove(intersection);
                }
                GetIntersectedElements(elements, group);
            }
        }
    }
}
