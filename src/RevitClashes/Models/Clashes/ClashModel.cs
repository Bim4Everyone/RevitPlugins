using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Autodesk.Revit.DB;

using DevExpress.Mvvm.DataAnnotations;

using dosymep.Revit;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Extensions;

namespace RevitClashDetective.Models.Clashes {
    internal class ClashModel : IEquatable<ClashModel> {
        private RevitRepository _revitRepository;


        [JsonConstructor]
        public ClashModel() { }

        public ClashModel(RevitRepository revitRepository, Element mainElement, Element otherElement)
            : this(revitRepository, mainElement, Transform.Identity, otherElement, Transform.Identity) {
        }

        public ClashModel(
            RevitRepository revitRepository,
            Element mainElement,
            Transform mainElementTransform,
            Element otherElement,
            Transform otherElementTransform) {

            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            if(mainElement is null) { throw new ArgumentNullException(nameof(mainElement)); }
            if(mainElementTransform is null) { throw new ArgumentNullException(nameof(mainElementTransform)); }
            if(otherElement is null) { throw new ArgumentNullException(nameof(otherElement)); }
            if(otherElementTransform is null) { throw new ArgumentNullException(nameof(otherElementTransform)); }

            MainElement = new ElementModel(mainElement, mainElementTransform);
            OtherElement = new ElementModel(otherElement, otherElementTransform);
        }


        public ClashStatus ClashStatus { get; set; }
        public ElementModel MainElement { get; set; }
        public ElementModel OtherElement { get; set; }


        public ClashModel SetRevitRepository(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            return this;
        }

        public bool IsValid(ICollection<string> documentNames) {
            var clashDocuments = new[] { MainElement.DocumentName, OtherElement.DocumentName };
            var clashElements = new[] {_revitRepository.GetElement(MainElement.DocumentName, MainElement.Id),
                                       _revitRepository.GetElement(OtherElement.DocumentName, OtherElement.Id)};

            return clashDocuments.All(item => documentNames.Any(d => d.Contains(item))) && clashElements.Any(item => item != null)
                   && clashElements.All(item => item?.GetTypeId().IsNotNull() == true);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ClashModel);
        }

        public override int GetHashCode() {
            int hashCode = 2096115351;
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementModel>.Default.GetHashCode(MainElement);
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementModel>.Default.GetHashCode(OtherElement);
            return hashCode;
        }

        public bool Equals(ClashModel other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return (Equals(MainElement, other.MainElement)
                && Equals(OtherElement, other.OtherElement))
                || (Equals(MainElement, other.OtherElement)
                && Equals(OtherElement, other.MainElement));
        }

        /// <summary>
        /// Возвращает данные о коллизии. Необходимо, чтобы <see cref="_revitRepository"/> не был null.
        /// Назначить это поле можно через метод <see cref="SetRevitRepository(RevitRepository)"/>
        /// </summary>
        /// <returns>Объект с данными о коллизии</returns>
        /// <exception cref="ArgumentNullException">Исключение, если <see cref="_revitRepository"/> null</exception>
        public ClashData GetClashData() {
            if(_revitRepository is null) {
                throw new ArgumentNullException(
                    "Для получения данных о коллизии необходимо назначить репозиторий активного документа Ревита",
                    nameof(_revitRepository));
            }

            try {
                List<Solid> mainSolids = GetMainSolids();
                List<Solid> otherSolids = GetOtherSolids();

                double intersectionVolume = GetIntersectionVolume(mainSolids, otherSolids);

                double mainSolidsV = mainSolids.Sum(s => s.Volume);
                double otherSolidsV = otherSolids.Sum(s => s.Volume);
                return new ClashData(mainSolidsV, otherSolidsV, intersectionVolume);
            } catch(NullReferenceException) {
                return new ClashData();
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                return new ClashData();
            }
        }


        private double GetIntersectionVolume(List<Solid> first, List<Solid> second) {
            double intersection = 0;
            try {
                foreach(var solid1 in first) {
                    foreach(var solid2 in second) {
                        intersection += BooleanOperationsUtils.ExecuteBooleanOperation(
                            solid1,
                            solid2,
                            BooleanOperationsType.Intersect).Volume;
                    }
                }
            } catch(NullReferenceException) {
                intersection = 0;
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                intersection = 0;
            }
            return intersection;
        }

        private List<Solid> GetMainSolids() {
            return MainElement.GetElement(_revitRepository.DocInfos).GetSolids();
        }

        private List<Solid> GetOtherSolids() {
            Transform transform = GetMainTransform()
                    .GetTransitionMatrix(GetOtherTransform());
            return OtherElement
                    .GetElement(_revitRepository.DocInfos)
                    .GetSolids()
                    .Select(s => SolidUtils.CreateTransformed(s, transform))
                    .ToList();
        }

        private Transform GetMainTransform() {
            return MainElement.TransformModel.GetTransform();
        }

        private Transform GetOtherTransform() {
            return OtherElement.TransformModel.GetTransform();
        }
    }

    internal enum ClashStatus {
        [Display(Name = "Активно"), Image("pack://application:,,,/DevExpress.Images.v21.2;component/Images/XAF/State_Priority_High.png")]
        Active,
        [Display(Name = "Проанализировано"), Image("pack://application:,,,/DevExpress.Images.v21.2;component/Images/XAF/State_Priority_Low.png")]
        Analized,
        [Display(Name = "Исправлено"), Image("pack://application:,,,/DevExpress.Images.v21.2;component/Images/XAF/State_Priority_Normal.png")]
        Solved
    }
}
