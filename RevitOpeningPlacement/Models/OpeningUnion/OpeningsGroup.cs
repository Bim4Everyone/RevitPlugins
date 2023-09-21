using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.OpeningUnion {
    /// <summary>
    /// Класс для группы экземпляров семейств заданий на отверстия.
    /// Экземпляры семейств не повторяются внутри группы.
    /// </summary>
    internal class OpeningsGroup {
        private readonly HashSet<OpeningMepTaskOutcoming> _elements = new HashSet<OpeningMepTaskOutcoming>();
        private bool _allOpeningsInMultilayerConstruction = true;


        public OpeningsGroup(ICollection<OpeningMepTaskOutcoming> openingTasks) {
            AddOpenings(openingTasks);
        }


        /// <summary>
        /// Флаг, показывающий, образуют ли все задания на отверстия в группе цилиндр
        /// </summary>
        public bool IsCylinder => _allOpeningsInMultilayerConstruction
            && (_elements.All(opening => opening.OpeningType == OpeningType.WallRound)
            || _elements.All(opening => opening.OpeningType == OpeningType.FloorRound));

        /// <summary>
        /// Задания на отверстия, расположенные в группе
        /// </summary>
        public ICollection<OpeningMepTaskOutcoming> Elements { get { return _elements; } }


        /// <summary>
        /// Возвращает генератор задания на отверстие, 
        /// </summary>
        /// <param name="revitRepository"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">В группе находится менее 2-х заданий на отверстия</exception>
        public OpeningPlacer GetOpeningPlacer(RevitRepository revitRepository) {
            try {
                OpeningPlacer placer;
                if(_elements.Any(task => (task.OpeningType == OpeningType.FloorRound) || (task.OpeningType == OpeningType.FloorRectangle))) {
                    placer = new FloorOpeningGroupPlacerInitializer().GetPlacer(revitRepository, this);
                } else {
                    placer = new WallOpeningGroupPlacerInitializer().GetPlacer(revitRepository, this);
                }
                return placer;

            } catch(ArgumentNullException) {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Добавляет поданный экземпляр семейства в текущую группу элементов, если этот экземпляр еще не добавлен в нее, 
        /// а также объединяет Solid поданного элемента с Solid из списка уже добавленных <see cref="_solids">солидов элементов</see>, если они пересекаются.
        /// </summary>
        /// <param name="openingTask">Экземпляр семейства задания на отверстие для добавления</param>
        public bool AddOpening(OpeningMepTaskOutcoming openingTask) {
            _allOpeningsInMultilayerConstruction =
                _allOpeningsInMultilayerConstruction
                && OpeningsHaveTheSameType(openingTask)
                && OpeningsLocatedOnTheSameLine(openingTask)
                && IsTouchingOpening(openingTask);

            return _elements.Add(openingTask);
        }

        /// <summary>
        /// Добавляет те исходящие задания на отверстия в группу, которых еще нет в группе
        /// </summary>
        /// <param name="openingTasks"></param>
        public void AddOpenings(ICollection<OpeningMepTaskOutcoming> openingTasks) {
            foreach(var openingTask in openingTasks) {
                AddOpening(openingTask);
            }
        }

        /// <summary>
        /// Проверяет, содержится ли данное исходящее задание на отверстие в группе
        /// </summary>
        /// <param name="openingTask"></param>
        /// <returns></returns>
        public bool Contains(OpeningMepTaskOutcoming openingTask) {
            return _elements.Contains(openingTask);
        }

        /// <summary>
        /// Проверяет, содержится ли какое-либо исходящее задание на отверстие в группе
        /// </summary>
        /// <param name="openingMepTasks"></param>
        /// <returns></returns>
        public bool ContainsAny(ICollection<OpeningMepTaskOutcoming> openingMepTasks) {
            return _elements.Overlaps(openingMepTasks);
        }

        /// <summary>
        /// Возвращает объединенное значение имен систем группы заданий на отверстия
        /// </summary>
        /// <returns></returns>
        public string GetMepSystems() {
            return string.Join("; ", _elements.Select(element => element.MepSystem.Trim()).Distinct());
        }

        /// <summary>
        /// Возвращает объединенное значение описания группы заданий на отверстия
        /// </summary>
        /// <returns></returns>
        public string GetDescription() {
            HashSet<string> result = new HashSet<string>();
            foreach(var opening in _elements) {
                string[] descriptionItems = opening.Description.Split(';');
                foreach(string descriptionItem in descriptionItems) {
                    result.Add(descriptionItem.Trim());
                }
            }
            return string.Join("; ", result);
        }


        /// <summary>
        /// Проверяет, все ли задания на отверстия в группе имеют такой же тип проема, как и проверяемое
        /// </summary>
        /// <param name="otherOpeningTask">Проверяемое задание на отверстие</param>
        /// <returns>True, если все ли задания на отверстия в группе имеют такой же тип проема, как и проверяемое</returns>
        private bool OpeningsHaveTheSameType(OpeningMepTaskOutcoming otherOpeningTask) {
            return _elements.All(existingOpening => existingOpening.OpeningType == otherOpeningTask.OpeningType);
        }

        /// <summary>
        /// Проверяет, есть ли в группе хотя бы одно задание на отверстие, которое имеет общую грань с проверяемым
        /// </summary>
        /// <param name="otherOpeningTask">Проверяемое задание на отверстие</param>
        /// <returns>True, если группа пустая или если среди отверстий в группе есть хотя бы одно, которое имеет общую грань с проверяемым, иначе False</returns>
        private bool IsTouchingOpening(OpeningMepTaskOutcoming otherOpeningTask) {
            return (_elements.Count == 0) || _elements.Any(existingOpening => existingOpening.HasCommonFace(otherOpeningTask));
        }

        /// <summary>
        /// Проверяет, расположены ли задания на отверстия в группе на той же прямой, что и проверяемое
        /// </summary>
        /// <param name="otherOpeningTask">Проверяемое задание на отверстие</param>
        /// <returns>True, если задания на отверстия в группе расположены на той же прямой, что и проверяемое, иначе False</returns>
        private bool OpeningsLocatedOnTheSameLine(OpeningMepTaskOutcoming otherOpeningTask) {
            if(_elements.Count < 2) { return true; }
            XYZ firstToOtherDirection = GetDirectionBetween(_elements.First(), otherOpeningTask);
            XYZ lastToOtherDirection = GetDirectionBetween(_elements.Last(), otherOpeningTask);
            double angle = firstToOtherDirection.AngleTo(lastToOtherDirection);
            bool angleIsZero = Math.Round(angle, 9) == 0;
            bool angleIsPi = Math.Round(angle - Math.PI, 9) == 0;
            return angleIsZero || angleIsPi;
        }

        /// <summary>
        /// Возвращает вектор, направленный от первого задания на отверстие ко второму
        /// </summary>
        /// <param name="firstOpeningTask">Первое задание на отверстие</param>
        /// <param name="secondOpeningTask">Второе задание на отверстие</param>
        /// <returns>Нормализованный вектор, направленный от первого задания на отверстие ко второму</returns>
        private XYZ GetDirectionBetween(OpeningMepTaskOutcoming firstOpeningTask, OpeningMepTaskOutcoming secondOpeningTask) {
            return (secondOpeningTask.Location - firstOpeningTask.Location).Normalize();
        }
    }
}
