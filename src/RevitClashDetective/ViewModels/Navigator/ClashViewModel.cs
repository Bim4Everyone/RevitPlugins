using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

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

            (IntersectionPercentage, IntersectionVolume) = GetIntersectionData(clash);

            ClashStatus = clash.ClashStatus;
            Clash = clash;
            Clash.SetRevitRepository(_revitRepository);
        }


        public ClashStatus ClashStatus {
            get => _clashStatus;
            set => RaiseAndSetIfChanged(ref _clashStatus, value);
        }

        public string FirstName { get; }

        public string FirstDocumentName { get; }

        public string FirstLevel { get; }

        public string FirstCategory { get; }

        public string SecondName { get; }

        public string SecondLevel { get; }

        public string SecondDocumentName { get; }

        public string SecondCategory { get; }

        /// <summary>
        /// Процент пересечения относительно объема меньшего элемента коллизии
        /// </summary>
        public double IntersectionPercentage { get; }

        /// <summary>
        /// Объем пересечения в м3
        /// </summary>
        public double IntersectionVolume { get; }

        public ClashModel Clash { get; }

        public IEnumerable<ElementId> GetElementIds(string docTitle) {
            if(docTitle.Contains(FirstDocumentName)) {
                yield return Clash.MainElement.Id;
            }
            if(docTitle.Contains(SecondDocumentName)) {
                yield return Clash.OtherElement.Id;
            }
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
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(Clash.MainElement.Id);
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(Clash.OtherElement.Id);
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
                && Clash.MainElement.Id == other.Clash.MainElement.Id
                && Clash.OtherElement.Id == other.Clash.OtherElement.Id
                && FirstName == other.FirstName
                && FirstDocumentName == other.FirstDocumentName
                && FirstLevel == other.FirstLevel
                && FirstCategory == other.FirstCategory
                && SecondName == other.SecondName
                && SecondLevel == other.SecondLevel
                && SecondDocumentName == other.SecondDocumentName
                && SecondCategory == other.SecondCategory;
        }


        private (double Percentage, double Volume) GetIntersectionData(ClashModel clashModel) {
            if(clashModel is null) {
                throw new ArgumentNullException(nameof(clashModel));
            }

            ClashData clashData = clashModel
                .SetRevitRepository(_revitRepository)
                .GetClashData();

            double minVolume = Math.Min(clashData.MainElementVolume, clashData.OtherElementVolume);
            return (
                Math.Round(clashData.ClashVolume / minVolume * 100, 2),
                Math.Round(_revitRepository.ConvertToM3(clashData.ClashVolume), 6));
        }
    }
}
