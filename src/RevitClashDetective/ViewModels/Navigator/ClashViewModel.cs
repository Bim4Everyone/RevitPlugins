using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.ViewModels.Navigator {
    internal class ClashViewModel : BaseViewModel, IEquatable<ClashViewModel> {
        private ClashStatus _clashStatus;
        private string _clashName;
        private readonly RevitRepository _revitRepository;

        public ClashViewModel(RevitRepository revitRepository, ClashModel clash) {
            _revitRepository = revitRepository;

            ClashName = clash.Name;

            FirstCategory = clash.MainElement.Category;
            FirstTypeName = clash.MainElement.Name;
            FirstFamilyName = clash.MainElement.FamilyName;
            FirstDocumentName = clash.MainElement.DocumentName;
            FirstLevel = clash.MainElement.Level;

            SecondCategory = clash.OtherElement.Category;
            SecondTypeName = clash.OtherElement.Name;
            SecondFamilyName = clash.OtherElement.FamilyName;
            SecondLevel = clash.OtherElement.Level;
            SecondDocumentName = clash.OtherElement.DocumentName;

            SetIntersectionData(clash);

            ClashStatus = clash.ClashStatus;
            Clash = clash;
            Clash.SetRevitRepository(_revitRepository);
        }


        public ClashStatus ClashStatus {
            get => _clashStatus;
            set => RaiseAndSetIfChanged(ref _clashStatus, value);
        }

        public string ClashName {
            get => _clashName;
            set => RaiseAndSetIfChanged(ref _clashName, value);
        }

        public ClashData ClashData { get; private set; }

        public string FirstTypeName { get; }

        public string FirstFamilyName { get; }

        public string FirstDocumentName { get; }

        public string FirstLevel { get; }

        public string FirstCategory { get; }

        public string SecondTypeName { get; }

        public string SecondFamilyName { get; }

        public string SecondLevel { get; }

        public string SecondDocumentName { get; }

        public string SecondCategory { get; }

        /// <summary>
        /// Процент пересечения относительно объема первого элемента коллизии
        /// </summary>
        public double MainElementIntersectionPercentage { get; private set; }

        /// <summary>
        /// Процент пересечения относительно объема второго элемента коллизии
        /// </summary>
        public double SecondElementIntersectionPercentage { get; private set; }

        /// <summary>
        /// Объем пересечения в м3
        /// </summary>
        public double IntersectionVolume { get; private set; }

        public ClashModel Clash { get; }


        public ClashModel GetClashModel() {
            Clash.ClashStatus = ClashStatus;
            Clash.Name = ClashName;
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
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstTypeName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstDocumentName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstLevel);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FirstCategory);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SecondTypeName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SecondLevel);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SecondDocumentName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SecondCategory);
            return hashCode;
        }

        public bool Equals(ClashViewModel other) {
            return other != null
                && Clash.MainElement.Id == other.Clash.MainElement.Id
                && Clash.OtherElement.Id == other.Clash.OtherElement.Id
                && FirstTypeName == other.FirstTypeName
                && FirstDocumentName == other.FirstDocumentName
                && FirstLevel == other.FirstLevel
                && FirstCategory == other.FirstCategory
                && SecondTypeName == other.SecondTypeName
                && SecondLevel == other.SecondLevel
                && SecondDocumentName == other.SecondDocumentName
                && SecondCategory == other.SecondCategory;
        }

        private void SetIntersectionData(ClashModel clashModel) {
            if(clashModel is null) {
                throw new ArgumentNullException(nameof(clashModel));
            }

            ClashData = clashModel
                .SetRevitRepository(_revitRepository)
                .GetClashData();

            IntersectionVolume = Math.Round(_revitRepository.ConvertToM3(ClashData.ClashVolume), 6);
            MainElementIntersectionPercentage =
                Math.Round(ClashData.ClashVolume / ClashData.MainElementVolume * 100, 2);
            SecondElementIntersectionPercentage =
                 Math.Round(ClashData.ClashVolume / ClashData.OtherElementVolume * 100, 2);
        }
    }
}
