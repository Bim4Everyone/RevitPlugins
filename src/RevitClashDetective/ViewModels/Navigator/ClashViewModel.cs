using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Extensions;

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

            try {
                Solid firstSolid = GetFirstSolid(clashModel);
                Solid secondSolid = GetSecondSolid(clashModel);

                Solid intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                    firstSolid,
                    secondSolid,
                    BooleanOperationsType.Intersect);

                double minVolume = firstSolid.Volume < secondSolid.Volume
                    ? firstSolid.Volume
                    : secondSolid.Volume;
                double intersectionVolume = intersection.Volume;
                return (Math.Round(intersectionVolume / minVolume * 100, 2),
                    Math.Round(ConvertToM3(intersection.Volume), 6));
            } catch(NullReferenceException) {
                return (0, 0);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                return (0, 0);
            }
        }

        private Solid GetFirstSolid(ClashModel clashModel) {
            return clashModel.MainElement.GetElement(_revitRepository.DocInfos).GetSolid();
        }

        private Solid GetSecondSolid(ClashModel clashModel) {
            return SolidUtils.CreateTransformed(
                clashModel.OtherElement
                    .GetElement(_revitRepository.DocInfos)
                    .GetSolid(),
                GetFirstTransform(clashModel)
                    .GetTransitionMatrix(GetSecondTransform(clashModel))
                );
        }

        private Transform GetFirstTransform(ClashModel clashModel) {
            return GetTransform(clashModel.MainElement.TransformModel);
        }

        private Transform GetSecondTransform(ClashModel clashModel) {
            return GetTransform(clashModel.OtherElement.TransformModel);
        }

        private Transform GetTransform(TransformModel transformModel) {
            Transform transform = Transform.Identity;
            transform.Origin = transformModel.Origin.GetXYZ();
            transform.BasisX = transformModel.BasisX.GetXYZ();
            transform.BasisY = transformModel.BasisY.GetXYZ();
            transform.BasisZ = transformModel.BasisZ.GetXYZ();
            return transform;
        }

        /// <summary>
        /// Конвертирует кубические футы в кубические метры
        /// </summary>
        /// <param name="cubeFeet">Кубические футы</param>
        /// <returns>Кубические метры</returns>
        public double ConvertToM3(double cubeFeet) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertFromInternalUnits(cubeFeet, DisplayUnitType.DUT_CUBIC_METERS);
#else
            return UnitUtils.ConvertFromInternalUnits(cubeFeet, UnitTypeId.CubicMeters);
#endif
        }
    }
}
