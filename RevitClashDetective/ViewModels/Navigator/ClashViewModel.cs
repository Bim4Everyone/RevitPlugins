using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.ViewModels.Navigator {
    internal class ClashViewModel : BaseViewModel, IEquatable<ClashViewModel> {
        private ClashStatus _clashStatus;
        private readonly RevitRepository _revitRepository;

        public ClashViewModel(RevitRepository revitRepository, ClashModel clash) {
            _revitRepository = revitRepository;

            FirstCategory = clash.MainElement.Category;
            FirstName = clash.MainElement.Name;
            FirstDocumentName = clash.MainElement.DocumentName;
            FirstLevel = clash.MainElement.Level;

            SecondCategory = clash.OtherElement.Category;
            SecondName = clash.OtherElement.Name;
            SecondLevel = clash.OtherElement.Level;
            SecondDocumentName = clash.OtherElement.DocumentName;

            ClashStatus = clash.ClashStatus;
            Clash = clash;
            Clash.SetRevitRepository(_revitRepository);
        }


        public ClashStatus ClashStatus {
            get => _clashStatus;
            set => this.RaiseAndSetIfChanged(ref _clashStatus, value);
        }

        public string FirstName { get; }

        public string FirstDocumentName { get; }

        public string FirstLevel { get; }

        public string FirstCategory { get; }

        public string SecondName { get; }

        public string SecondLevel { get; }

        public string SecondDocumentName { get; }

        public string SecondCategory { get; set; }
        public ClashModel Clash { get; }

        public IEnumerable<ElementId> GetElementIds(string docTitle) {
            if(docTitle.Contains(FirstDocumentName)) {
                yield return new ElementId(Clash.MainElement.Id);
            }
            if(docTitle.Contains(SecondDocumentName)) {
                yield return new ElementId(Clash.OtherElement.Id);
            }
        }

        public BoundingBoxXYZ GetBoundingBox() {
            return Clash.GetClashBoundingBox();
        }

        public ClashModel GetClashModel() {
            Clash.ClashStatus = ClashStatus;
            return Clash;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ClashViewModel);
        }

        public override int GetHashCode() {
            int hashCode = 635569250;
            hashCode = hashCode * -1521134295 + ClashStatus.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstDocumentName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstLevel);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstCategory);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SecondName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SecondLevel);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SecondDocumentName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SecondCategory);
            return hashCode;
        }

        public bool Equals(ClashViewModel other) {
            return other != null
                && ClashStatus == other.ClashStatus
                && FirstName == other.FirstName
                && FirstDocumentName == other.FirstDocumentName
                && FirstLevel == other.FirstLevel
                && FirstCategory == other.FirstCategory
                && SecondName == other.SecondName
                && SecondLevel == other.SecondLevel
                && SecondDocumentName == other.SecondDocumentName
                && SecondCategory == other.SecondCategory;
        }
    }
}