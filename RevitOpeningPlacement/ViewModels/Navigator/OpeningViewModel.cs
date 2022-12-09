using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    internal class OpeningViewModel : BaseViewModel, IEquatable<OpeningViewModel> {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Level { get; set; }
        public string TypeName { get; set; }
        public string FamilyName { get; set; }

        public static IEnumerable<OpeningViewModel> GetOpenings(RevitRepository revitRepository, OpeningsGroup group) {
            if(group.Elements.Count < 1) {
                yield break;
            }

            FamilyInstance firstOpening = group.Elements.Take(1).First();
            var firstOpeningViewModel = new OpeningViewModel() {
                Id = firstOpening.Id.IntegerValue,
                Level = revitRepository.GetLevelName(firstOpening),
                TypeName = firstOpening.Name,
                FamilyName = revitRepository.GetFamilyName(firstOpening)
            };

            var otherOpenings = group.Elements.Skip(1);
            if(otherOpenings.Any()) {
                firstOpeningViewModel.ParentId = firstOpeningViewModel.Id;
            }
            yield return firstOpeningViewModel;

            foreach(var opening in otherOpenings) {
                yield return new OpeningViewModel() {
                    Id = opening.Id.IntegerValue,
                    ParentId = firstOpening.Id.IntegerValue,
                    Level = revitRepository.GetLevelName(opening),
                    TypeName = opening.Name,
                    FamilyName = revitRepository.GetFamilyName(opening)
                };
            }
        }

        public override bool Equals(object obj) {
            return Equals(obj as OpeningViewModel);
        }

        public bool Equals(OpeningViewModel other) {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode() {
            return 2108858624 + Id.GetHashCode();
        }
    }
}
