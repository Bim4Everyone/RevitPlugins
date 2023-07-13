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

        /// <summary>
        /// Статус отработки задания на отверстие
        /// </summary>
        public OpeningTaskIncomingStatusEnum Status { get; set; } = OpeningTaskIncomingStatusEnum.NewTask;


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
            if(_familyInstance.IsExistsParam(paramName)) {
                value = _familyInstance.GetParamValue<string>(paramName);
            }
            return value;
        }
    }
}
