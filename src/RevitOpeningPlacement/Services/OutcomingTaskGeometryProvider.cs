using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Services {
    /// <summary>
    /// Класс предоставляет методы для получения геометрии из исходящего задания на отверстие.<br/>
    /// <br/>
    /// Алгоритмы класса исходят из утверждений, что:<br/>
    /// <br/>
    /// Круглое отверстие в стене - это прямой цилиндр, расположенный горизонтально.<br/>
    /// Точка вставки круглого отверстия в стене - это центр передней грани цилиндра.<br/>
    /// Круглое отверстие в перекрытии - это прямой цилиндр, расположенный вертикально.<br/>
    /// Точка вставки круглого отверстия в перекрытии - это центр верхней грани цилиндра.<br/>
    /// <br/>
    /// Прямоугольное отверстие в стене и прямоугольное отверстие в перекрытии - это прямоугольные параллелепипеды, 
    /// у которых отсутствует поворот относительно осей OY и OX.<br/>
    /// Точка вставки прямоугольного отверстия в стене - это середина нижнего переднего ребра параллелепипеда.<br/>
    /// Точка вставки прямоугольного отверстия в перекрытии - это центр верхней грани параллелепипеда.<br/>
    /// </summary>
    internal class OutcomingTaskGeometryProvider {
        public OutcomingTaskGeometryProvider() { }


        /// <summary>
        /// Находит фронтальную плоскость относительно сечения задания. 
        /// Направление вектора нормали плоскости внутрь задания.
        /// </summary>
        /// <param name="opening">Исходящее задание на отверстие</param>
        /// <returns>Фронтальная плоскость</returns>
        public Plane GetFrontPlane(OpeningMepTaskOutcoming opening) {
            var location = GetLocation(opening);
            switch(opening.OpeningType) {
                case Models.OpeningType.WallRound:
                case Models.OpeningType.WallRectangle:
                    var frontNormal = GetRotationAsVector(opening).Negate();
                    return Plane.CreateByNormalAndOrigin(frontNormal, location);
                case Models.OpeningType.FloorRound:
                case Models.OpeningType.FloorRectangle:
                    return Plane.CreateByNormalAndOrigin(-XYZ.BasisZ, location);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Находит заднюю плоскость относительно сечения задания. 
        /// Направление вектора нормали плоскости внутрь задания.
        /// </summary>
        /// <param name="opening">Исходящее задание на отверстие</param>
        /// <returns>Задняя плоскость</returns>
        public Plane GetBackPlane(OpeningMepTaskOutcoming opening) {
            var location = GetLocation(opening);
            var thickness = GetThickness(opening);
            switch(opening.OpeningType) {
                case Models.OpeningType.WallRound:
                case Models.OpeningType.WallRectangle:
                    var backNormal = GetRotationAsVector(opening);
                    var backCenter = location - backNormal * thickness;
                    return Plane.CreateByNormalAndOrigin(backNormal, backCenter);
                case Models.OpeningType.FloorRound:
                case Models.OpeningType.FloorRectangle:
                    var center = location - XYZ.BasisZ * thickness;
                    return Plane.CreateByNormalAndOrigin(XYZ.BasisZ, center);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Находит левую плоскость относительно сечения задания.
        /// </summary>
        /// <param name="opening">Исходящее задание на отверстие</param>
        /// <returns>Левая плоскость</returns>
        public Plane GetLeftPlane(OpeningMepTaskOutcoming opening) {
            var location = GetLocation(opening);
            var halfWidth = GetWidth(opening) / 2;
            var rotation = GetRotationAsVector(opening);
            switch(opening.OpeningType) {
                case OpeningType.WallRound:
                case OpeningType.WallRectangle:
                case OpeningType.FloorRound:
                case OpeningType.FloorRectangle:
                    var leftNormal = RotateClockwise90(rotation).Normalize();
                    var leftCenter = location + leftNormal * halfWidth;
                    return Plane.CreateByNormalAndOrigin(leftNormal, leftCenter);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Находит правую плоскость относительно сечения задания.
        /// </summary>
        /// <param name="opening">Исходящее задание на отверстие</param>
        /// <returns>Правую плоскость</returns>
        public Plane GetRightPlane(OpeningMepTaskOutcoming opening) {
            var location = GetLocation(opening);
            var halfWidth = GetWidth(opening) / 2;
            var rotation = GetRotationAsVector(opening);
            switch(opening.OpeningType) {
                case OpeningType.WallRound:
                case OpeningType.WallRectangle:
                case OpeningType.FloorRound:
                case OpeningType.FloorRectangle:
                    var rightNormal = RotateCounterClockwise90(rotation).Normalize();
                    var leftCenter = location + rightNormal * halfWidth;
                    return Plane.CreateByNormalAndOrigin(rightNormal, leftCenter);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Находит верхнюю плоскость относительно сечения задания.
        /// </summary>
        /// <param name="opening">Исходящее задание на отверстие</param>
        /// <returns>Верхняя плоскость</returns>
        public Plane GetTopPlane(OpeningMepTaskOutcoming opening) {
            var location = GetLocation(opening);
            var height = GetHeight(opening);
            var rotation = GetRotationAsVector(opening);
            switch(opening.OpeningType) {
                case OpeningType.WallRound:
                    var topCenter = location + height / 2 * XYZ.BasisZ;
                    return Plane.CreateByNormalAndOrigin(XYZ.BasisZ, topCenter);
                case OpeningType.WallRectangle:
                    topCenter = location + height * XYZ.BasisZ;
                    return Plane.CreateByNormalAndOrigin(XYZ.BasisZ, topCenter);
                case OpeningType.FloorRound:
                case OpeningType.FloorRectangle:
                    topCenter = location - height / 2 * rotation;
                    return Plane.CreateByNormalAndOrigin(-rotation, topCenter);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Находит нижнюю плоскость относительно сечения задания.
        /// </summary>
        /// <param name="opening">Исходящее задание на отверстие</param>
        /// <returns>Нижняя плоскость</returns>
        public Plane GetBottomPlane(OpeningMepTaskOutcoming opening) {
            var location = GetLocation(opening);
            var height = GetHeight(opening);
            var rotation = GetRotationAsVector(opening);
            switch(opening.OpeningType) {
                case OpeningType.WallRound:
                    var topCenter = location - height / 2 * XYZ.BasisZ;
                    return Plane.CreateByNormalAndOrigin(-XYZ.BasisZ, topCenter);
                case OpeningType.WallRectangle:
                    return Plane.CreateByNormalAndOrigin(-XYZ.BasisZ, location);
                case OpeningType.FloorRound:
                case OpeningType.FloorRectangle:
                    topCenter = location + height / 2 * rotation;
                    return Plane.CreateByNormalAndOrigin(rotation, topCenter);
                default:
                    throw new InvalidOperationException();
            }
        }


        private double GetWidth(OpeningMepTaskOutcoming opening) {
            var famInst = opening.GetFamilyInstance();
            switch(opening.OpeningType) {
                case OpeningType.WallRound:
                case OpeningType.FloorRound:
                    return famInst.GetParamValue<double>(RevitRepository.OpeningDiameter);
                case OpeningType.WallRectangle:
                case OpeningType.FloorRectangle:
                    return famInst.GetParamValue<double>(RevitRepository.OpeningWidth);
                default:
                    throw new InvalidOperationException();
            }
        }

        private double GetHeight(OpeningMepTaskOutcoming opening) {
            var famInst = opening.GetFamilyInstance();
            switch(opening.OpeningType) {
                case OpeningType.WallRound:
                case OpeningType.FloorRound:
                    return famInst.GetParamValue<double>(RevitRepository.OpeningDiameter);
                case OpeningType.WallRectangle:
                case OpeningType.FloorRectangle:
                    return famInst.GetParamValue<double>(RevitRepository.OpeningHeight);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Находит толщину задания в единицах Revit
        /// </summary>
        private double GetThickness(OpeningMepTaskOutcoming opening) {
            switch(opening.OpeningType) {
                case OpeningType.WallRound:
                case OpeningType.WallRectangle:
                case OpeningType.FloorRound:
                case OpeningType.FloorRectangle:
                    return opening.GetFamilyInstance().GetParamValue<double>(RevitRepository.OpeningThickness);
                default:
                    throw new InvalidOperationException();
            }
        }

        private XYZ GetLocation(OpeningMepTaskOutcoming opening) {
            return opening.Location;
        }

        private double GetRotation(OpeningMepTaskOutcoming opening) {
            return (opening.GetFamilyInstance().Location as LocationPoint).Rotation;
        }

        private XYZ GetRotationAsVector(OpeningMepTaskOutcoming opening) {
            var rotation = GetRotation(opening);
            return new XYZ(Math.Cos(rotation), Math.Sin(rotation), 0).Normalize();
        }

        /// <summary>
        /// Поворачивает горизонтальный вектор против часовой стрелки на 90 градусов
        /// </summary>
        private XYZ RotateCounterClockwise90(XYZ vector) {
            return new XYZ(-vector.Y, vector.X, 0);
        }

        /// <summary>
        /// Поворачивает горизонтальный вектор по часовой стрелке на 90 градусов
        /// </summary>
        private XYZ RotateClockwise90(XYZ vector) {
            return new XYZ(vector.Y, -vector.X, 0);
        }
    }
}
