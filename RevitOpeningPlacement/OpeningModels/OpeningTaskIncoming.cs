using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс, обозначающий экземпляры семейств заданий на отверстия из связанного файла-задания на отверстия,
    /// подгруженного в текущий документ получателя
    /// </summary>
    internal class OpeningTaskIncoming : ISolidProvider {
        /// <summary>
        /// Экземпляр семейства задания на отверстие
        /// </summary>
        private readonly FamilyInstance _familyInstance;


        /// <summary>
        /// Экземпляр семейства задания на отверстие, расположенного в связанном файле задания на отверстия
        /// </summary>
        /// <param name="openingTaskIncoming">Экземпляр семейства задания на отверстие из связанного файла</param>
        /// <param name="fileName">Название связанного файла-источника задания (без суффикса)</param>
        public OpeningTaskIncoming(FamilyInstance openingTaskIncoming, string fileName) {
            _familyInstance = openingTaskIncoming;
            Id = _familyInstance.Id.IntegerValue;
            Location = (_familyInstance.Location as LocationPoint).Point;
            FileName = fileName;
        }


        public string FileName { get; }

        /// <summary>
        /// Id экземпляра семейства задания на отверстие
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Точка расположения экземпляра семейства задания на отверстие
        /// </summary>
        public XYZ Location { get; private set; }

        /// <summary>
        /// Комментарий к входящему заданию на отверстие
        /// </summary>
        public string Comment { get; set; } = string.Empty;

        /// <summary>
        /// Флаг, обозначающий, принято ли задание получателем, или нет
        /// </summary>
        public bool IsAccepted { get; set; } = true;

        /// <summary>
        /// Статус отработки задания на отверстие
        /// </summary>
        public OpeningTaskIncomingStatusEnum Status { get; set; } = OpeningTaskIncomingStatusEnum.NewTask;


        public Solid GetSolid() {
            return _familyInstance.GetSolid();
        }

        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            return _familyInstance.GetBoundingBox().TransformBoundingBox(_familyInstance.GetTotalTransform().Inverse);
        }
    }
}
