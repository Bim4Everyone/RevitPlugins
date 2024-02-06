using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters {
    /// <summary>
    /// Класс, предоставляющий методы округления для габаритов чистовых отверстий АР/КР
    /// </summary>
    internal abstract class RealOpeningSizeValueGetter : RoundValueGetter {
        /// <summary>
        /// Значение округления высоты в мм
        /// </summary>
        private protected const int _heightRound = 10;

        /// <summary>
        /// Значение округления ширины в мм
        /// </summary>
        private protected const int _widthRound = 10;

        /// <summary>
        /// Значение округления диаметра в мм
        /// </summary>
        private protected const int _diameterRound = 10;


        protected RealOpeningSizeValueGetter() { }


        private protected BoundingBoxXYZ GetUnitedBox(ICollection<IOpeningTaskIncoming> incomingTasks) {
            return incomingTasks.Select(task => task.GetTransformedBBoxXYZ()).ToList().CreateUnitedBoundingBox();
        }

        /// <summary>
        /// Возвращает высоту задания на отверстие независимо от его формы
        /// </summary>
        /// <param name="incomingTask"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private protected double GetOpeningHeight(IOpeningTaskIncoming incomingTask) {
            if(incomingTask is null) {
                throw new ArgumentNullException(nameof(incomingTask));
            }
            return incomingTask.Height > 0 ? incomingTask.Height : incomingTask.Diameter;
        }

        /// <summary>
        /// Возвращает ширину задания на отверстие независимо от его формы
        /// </summary>
        /// <param name="incomingTask"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private protected double GetOpeningWidth(IOpeningTaskIncoming incomingTask) {
            if(incomingTask is null) {
                throw new ArgumentNullException(nameof(incomingTask));
            }
            return incomingTask.Width > 0 ? incomingTask.Width : incomingTask.Diameter;
        }

        /// <summary>
        /// Возвращает диаметр задания на отверстие независимо от его формы
        /// </summary>
        /// <param name="incomingTask"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private protected double GetOpeningDiameter(IOpeningTaskIncoming incomingTask) {
            if(incomingTask is null) {
                throw new ArgumentNullException(nameof(incomingTask));
            }
            return incomingTask.Diameter > 0 ? incomingTask.Diameter : incomingTask.Height;
        }
    }
}
