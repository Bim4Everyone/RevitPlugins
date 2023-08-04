using System;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс, обозначающий экземпляры семейств заданий на отверстия из связанного файла-задания на отверстия,
    /// подгруженного в текущий документ получателя
    /// </summary>
    internal class OpeningMepTaskIncoming : ISolidProvider {
        /// <summary>
        /// Экземпляр семейства задания на отверстие
        /// </summary>
        private readonly FamilyInstance _familyInstance;


        /// <summary>
        /// Экземпляр семейства задания на отверстие, расположенного в связанном файле задания на отверстия
        /// </summary>
        /// <param name="openingTaskIncoming">Экземпляр семейства задания на отверстие из связанного файла</param>
        public OpeningMepTaskIncoming(FamilyInstance openingTaskIncoming) {
            if(openingTaskIncoming is null) {
                throw new ArgumentNullException(nameof(openingTaskIncoming));
            }
            _familyInstance = openingTaskIncoming;

            Id = _familyInstance.Id.IntegerValue;
            Location = (_familyInstance.Location as LocationPoint).Point;
            FileName = _familyInstance.Document.PathName;

            Date = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningDate);
            MepSystem = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningMepSystem);
            Description = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningDescription);
            CenterOffset = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningOffsetCenter);
            BottomOffset = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningOffsetBottom);
            Diameter = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningDiameter);
            Height = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningHeight);
            Width = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningWidth);
            Thickness = GetFamilyInstanceStringParamValueOrEmpty(RevitRepository.OpeningThickness);
        }


        public string FileName { get; }

        /// <summary>
        /// Id экземпляра семейства задания на отверстие
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Точка расположения экземпляра семейства задания на отверстие
        /// </summary>
        public XYZ Location { get; }

        /// <summary>
        /// Комментарий к входящему заданию на отверстие
        /// </summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// Флаг, обозначающий, принято ли задание получателем, или нет
        /// </summary>
        public bool IsAccepted { get; set; } = true;

        public string Date { get; } = string.Empty;

        public string MepSystem { get; } = string.Empty;

        public string Description { get; } = string.Empty;

        public string CenterOffset { get; } = string.Empty;

        public string BottomOffset { get; } = string.Empty;

        public string Diameter { get; } = string.Empty;

        public string Width { get; } = string.Empty;

        public string Height { get; } = string.Empty;

        public string Thickness { get; } = string.Empty;

        /// <summary>
        /// Статус отработки задания на отверстие
        /// </summary>
        public OpeningTaskIncomingStatus Status { get; set; } = OpeningTaskIncomingStatus.NewTask;


        public FamilyInstance GetFamilyInstance() {
            return _familyInstance;
        }

        public Solid GetSolid() {
            return _familyInstance.GetSolid();
        }

        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            return _familyInstance.GetBoundingBox().TransformBoundingBox(_familyInstance.GetTotalTransform().Inverse);
        }

        private string GetFamilyInstanceStringParamValueOrEmpty(string paramName) {
            if(_familyInstance is null) {
                throw new ArgumentNullException(nameof(_familyInstance));
            }
            string value = string.Empty;
            if(_familyInstance.IsExistsSharedParam(paramName)) {
#if REVIT_2022_OR_GREATER
                if(_familyInstance.GetSharedParam(paramName).Definition.GetDataType() == SpecTypeId.Length) {
                    return Math.Round(UnitUtils.ConvertFromInternalUnits(_familyInstance.GetSharedParamValue<double>(paramName), UnitTypeId.Millimeters)).ToString();
                }
#elif REVIT_2021
                if(_familyInstance.GetSharedParam(paramName).Definition.ParameterType == ParameterType.Length) {
                    return Math.Round(UnitUtils.ConvertFromInternalUnits(_familyInstance.GetSharedParamValue<double>(paramName), UnitTypeId.Millimeters)).ToString();
                }
#else
                if(_familyInstance.GetSharedParam(paramName).Definition.UnitType == UnitType.UT_Length) {
                    return Math.Round(UnitUtils.ConvertFromInternalUnits(_familyInstance.GetSharedParamValue<double>(paramName), DisplayUnitType.DUT_MILLIMETERS)).ToString();
                }
#endif
                object paramValue = _familyInstance.GetParamValue(paramName);
                if(!(paramValue is null)) {
                    if(paramValue is double doubleValue) {
                        value = Math.Round(doubleValue).ToString();
                    } else {
                        value = paramValue.ToString();
                    }
                }
            }
            return value;
        }
    }
}
