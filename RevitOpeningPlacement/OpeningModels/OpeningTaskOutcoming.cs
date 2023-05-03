using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс, обозначающий экземпляры семейств заданий на отверстия, 
    /// размещаемые в файлах-источниках заданий на отверстия для последующей передачи этих заданий получателю
    /// </summary>
    internal class OpeningTaskOutcoming : ISolidProvider {
        /// <summary>
        /// Экземпляр семейства задания на отверстие
        /// </summary>
        private readonly FamilyInstance _familyInstance;


        /// <summary>
        /// Создает экземпляр класса <see cref="OpeningTaskOutcoming"/>
        /// </summary>
        /// <param name="openingTaskOutcoming">Экземпляр семейства задания на отверстие, расположенного в текущем документе Revit</param>
        public OpeningTaskOutcoming(FamilyInstance openingTaskOutcoming) {
            _familyInstance = openingTaskOutcoming;
            Id = _familyInstance.Id.IntegerValue;
            Location = (_familyInstance.Location as LocationPoint).Point;
        }


        /// <summary>
        /// Id экземпляра семейства задания на отверстие
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Точка расположения экземпляра семейства задания на отверстие
        /// </summary>
        public XYZ Location { get; private set; }

        /// <summary>
        /// Флаг, обозначающий, удален ли экземпляр семейства задания на отверстие из проекта
        /// </summary>
        public bool IsRemoved { get; private set; } = false;

        /// <summary>
        /// Флаг, обозначающий, пересекается ли экземпляр семейства задания на отверстие с каким-либо элементом из текущего проекта, 
        /// для которого (элемента) и был создан этот экземпляр семейства задания на отверстие.
        /// 
        /// Например, в файле инженерных систем, экземпляр семейства задания должен пересекаться с каким-то элементом этих инженерных систем.
        /// </summary>
        public bool HasPurpose { get; set; } = false;


        /// <summary>
        /// Возвращает Solid экземпляра семейства задания на отверстие с трансформированными координатами
        /// </summary>
        /// <returns></returns>
        public Solid GetSolid() {
            return _familyInstance.GetSolid();
        }

        /// <summary>
        /// Возвращает BoundingBoxXYZ с учетом расположения <see cref="_familyInstance">элемента</see> в файле Revit
        /// </summary>
        /// <returns></returns>
        public BoundingBoxXYZ GetTransformedBBoxXYZ() {
            return _familyInstance.GetBoundingBox();
        }
    }
}
