using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    /// <summary>
    /// Класс для проверки наличия описательных параметров в семействах заданий на отверстия. Для проверки наличия семейств в активном документе использовать <see cref="FamilyAndTypeChecker"/>
    /// </summary>
    internal class FamiliesParametersChecker : IChecker {
        private readonly RevitRepository _revitRepository;


        /// <summary>
        /// Конструктор класса для проверки наличия описательных параметров в семействах заданий на отверстия.
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public FamiliesParametersChecker(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }


        public string GetErrorMessage() {
            StringBuilder message = new StringBuilder();
            foreach(OpeningType openingType in Enum.GetValues(typeof(OpeningType))) {
                var family = _revitRepository.GetOpeningTaskFamily(openingType);
                var notExistentParameters = GetNotExistentParameters(_revitRepository, family);

                if(notExistentParameters.Count > 0) {
                    message.AppendLine($"У семейства \"{family.Name}\" отсутствуют общие параметры:");
                    foreach(var paramName in notExistentParameters) {
                        message.AppendLine(paramName);
                    }
                    message.AppendLine();
                }
            }
            return message.ToString();
        }

        public bool IsCorrect() {
            foreach(OpeningType openingType in Enum.GetValues(typeof(OpeningType))) {
                var family = _revitRepository.GetOpeningTaskFamily(openingType);
                if(!HasAllParameters(_revitRepository, family)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Возвращает коллекцию названий параметров, которые отсутствуют у семейства
        /// </summary>
        /// <param name="revitRepository">Репозиторий активного документа Revit, в который загружено семейство</param>
        /// <param name="family">Семейство для проверки</param>
        /// <returns></returns>
        private ICollection<string> GetNotExistentParameters(RevitRepository revitRepository, Family family) {
            OpeningType type = RevitRepository.GetOpeningType(family.Name);
            var paramNames = GetRequiredParameters(type);
            var familyParameterNames = GetFamilySharedParameterNames(revitRepository, family);

            List<string> notExistentParameters = new List<string>();
            foreach(string paramName in paramNames) {
                if(!familyParameterNames.Contains(paramName)) {
                    notExistentParameters.Add(paramName);
                }
            }
            return notExistentParameters;
        }

        /// <summary>
        /// Проверяет, присутствуют ли в семействе все необходимые параметры
        /// </summary>
        /// <param name="revitRepository">Репозиторий активного документа Revit, в который загружено семейство</param>
        /// <param name="family">Семейство для проверки</param>
        /// <returns></returns>
        private bool HasAllParameters(RevitRepository revitRepository, Family family) {
            OpeningType type = RevitRepository.GetOpeningType(family.Name);
            var paramNames = GetRequiredParameters(type);
            var familyParameterNames = GetFamilySharedParameterNames(revitRepository, family);

            foreach(string paramName in paramNames) {
                if(!familyParameterNames.Contains(paramName)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Возвращает коллекцию названий общих параметров документа семейства
        /// </summary>
        /// <param name="revitRepository">Репозиторий активного документа Revit, в который загружено семейство</param>
        /// <param name="family">Семейство, загруженное в активный документ</param>
        /// <returns></returns>
        private ICollection<string> GetFamilySharedParameterNames(RevitRepository revitRepository, Family family) {
            using(var familyDoc = revitRepository.EditFamily(family)) {
                string[] paramNames = familyDoc.FamilyManager
                    .GetParameters()
                    .Where(param => param.IsShared)
                    .Select(param => param.Definition.Name)
                    .ToArray();
                familyDoc.Close(false);
                return paramNames;
            }
        }

        /// <summary>
        /// Возвращает коллекцию названий параметров, которые должны присутствовать у семейства с заданным <see cref="OpeningType">типом отверстия</see>
        /// </summary>
        /// <param name="openingType">Тип отверстия</param>
        /// <returns></returns>
        private ICollection<string> GetRequiredParameters(OpeningType openingType) {
            // начальный список с названиями параметров, присутствующих во всех семействах
            var paramNames = new List<string> {
                RevitRepository.OpeningDate,
                RevitRepository.OpeningDescription,
                RevitRepository.OpeningMepSystem,
                RevitRepository.OpeningAuthor,
                RevitRepository.OpeningThickness,
                RevitRepository.OpeningOffsetBottom,
                RevitRepository.OpeningIsManuallyPlaced,
                RevitRepository.OpeningOffsetBottomAdsk,
                RevitRepository.OpeningOffsetFromLevelAdsk,
                RevitRepository.OpeningLevelOffsetAdsk
            };
            // добавление специфических параметров семейств
            switch(openingType) {
                case OpeningType.WallRound:
                    paramNames.Add(RevitRepository.OpeningDiameter);
                    paramNames.Add(RevitRepository.OpeningOffsetCenter);
                    break;
                case OpeningType.WallRectangle:
                    paramNames.Add(RevitRepository.OpeningHeight);
                    paramNames.Add(RevitRepository.OpeningWidth);
                    paramNames.Add(RevitRepository.OpeningOffsetCenter);
                    break;
                case OpeningType.FloorRound:
                    paramNames.Add(RevitRepository.OpeningDiameter);
                    break;
                case OpeningType.FloorRectangle:
                    paramNames.Add(RevitRepository.OpeningHeight);
                    paramNames.Add(RevitRepository.OpeningWidth);
                    break;
            }
            return paramNames;
        }
    }
}
