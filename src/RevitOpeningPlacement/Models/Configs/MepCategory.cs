using System;
using System.Collections.Generic;
using System.Linq;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Evaluators;
using RevitClashDetective.Models.FilterModel;

namespace RevitOpeningPlacement.Models.Configs {
    /// <summary>
    /// Класс для обертки настроек расстановки заданий на отверстия для элементов категории инженерных систем
    /// </summary>
    internal class MepCategory : IEquatable<MepCategory> {
        public const int DefaultRoundingMm = 50;
        public const int DefaultMinSizeMm = 0;
        public const int DefaultMaxSizeMm = 10000;
        public const int DefaultOffsetMm = 50;
        public const string PipeImageSource = "../Resources/pipe.png";
        public const string RectDuctImageSource = "../Resources/rectangleDuct.png";
        public const string RoundDuctImageSource = "../Resources/roundDuct.png";
        public const string CableTrayImageSource = "../Resources/tray.png";
        public const string ConduitImageSource = "../Resources/conduit.png";


        /// <summary>
        /// Конструктор настроек расстановки заданий на отверстия для дисциплины инженерных систем
        /// </summary>
        /// <param name="name">Название дисциплины инженерных систем из списка <see cref="RevitRepository.MepCategoryNames"/></param>
        /// <param name="imageSource">Путь к изображению для отображения в интерфейсе</param>
        /// <param name="isRound">Является ли данная дисциплина инженерных систем круглой</param>
        /// <exception cref="ArgumentException">Исключение, если обязательные строковые параметры пустые</exception>
        [JsonConstructor]
        public MepCategory(string name, string imageSource, bool isRound) {
            if(string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException($"'{nameof(name)}' не может быть null или состоять только из пробелов.", nameof(name));
            }

            if(!RevitRepository.MepCategoryNames.Values.Contains(name)) {
                throw new ArgumentException($"'{nameof(name)}' не допустимое название.", nameof(name));
            }

            if(string.IsNullOrWhiteSpace(imageSource)) {
                throw new ArgumentException($"'{nameof(imageSource)}' не может быть null или состоять только из пробелов.", nameof(imageSource));
            }

            Name = name;
            ImageSource = imageSource;
            IsRound = isRound;
        }


        /// <summary>
        /// Является ли сечение элементов данной категории круглым (трубы, воздуховоды круглого сечения и т.п.)
        /// </summary>
        public bool IsRound { get; }

        /// <summary>
        /// Выбрана ли данная категория для поиска пересечений и последующей расстановки отверстий
        /// </summary>
        public bool IsSelected { get; set; } = true;

        /// <summary>
        /// Название категории
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Иконка
        /// </summary>
        public string ImageSource { get; }

        /// <summary>
        /// Минимальные значения габаритов сечения элементов данной категории для последующей проверки на пересечения и расстановки отверстий
        /// </summary>
        public SizeCollection MinSizes { get; set; } = new SizeCollection();

        /// <summary>
        /// Значения отступов от габаритов сечения
        /// </summary>
        public List<Offset> Offsets { get; set; } = new List<Offset>() {
                new Offset() {
                    From = DefaultMinSizeMm,
                    To = DefaultMaxSizeMm,
                    OffsetValue = DefaultOffsetMm,
                    OpeningTypeName = RevitRepository.OpeningTaskTypeName[OpeningType.WallRectangle]
                }
            };

        /// <summary>
        /// Категории конструкций (стены, перекрытия и т.п.), с которыми нужно выполнять проверку на пересечения и расстановку отверстий
        /// </summary>
        public List<StructureCategory> Intersections { get; set; } = new List<StructureCategory>(){
                new StructureCategory(){
                    Name = RevitRepository.StructureCategoryNames[StructureCategoryEnum.Wall],
                    IsSelected = true },
                new StructureCategory(){
                    Name = RevitRepository.StructureCategoryNames[StructureCategoryEnum.Floor],
                    IsSelected = true
                }
            };

        /// <summary>
        /// Значение округления габаритов итогового размещенного задания на отверстие в месте пересечения элемента данной категории и конструктивного элемента
        /// </summary>
        public int Rounding { get; set; } = DefaultRoundingMm;

        /// <summary>
        /// Правила фильтрации элементов данной категории
        /// </summary>
        public Set Set { get; set; } = new Set() {
            SetEvaluator = SetEvaluatorUtils.GetEvaluators().First(),
            Criteria = new List<Criterion>()
        };


        /// <summary>
        /// Возвращает значение отступа (суммарно с двух сторон) от габаритов элемента инженерной системы в единицах Revit
        /// </summary>
        /// <param name="size">Габарит инженерного элемента в единицах Revit</param>
        /// <returns></returns>
        public double GetOffsetValue(double size) {
            return GetOffsetTransformedToInternalUnit(size)?.OffsetValue * 2 ?? 0;
        }

        /// <summary>
        /// Возвращает настройки отступов для заданного габарита инженерной системы с размерами в единицах Revit
        /// </summary>
        /// <param name="size">Габарит инженерного элемента в единицах Revit</param>
        /// <returns></returns>
        public Offset GetOffsetTransformedToInternalUnit(double size) {
            return Offsets.Select(item => item.GetTransformedToInternalUnit())
                .FirstOrDefault(item => item.From <= size && item.To >= size);
        }

        public override bool Equals(object obj) {
            return (obj != null)
                && (obj is MepCategory mepCategory)
                && Equals(mepCategory);
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }

        public bool Equals(MepCategory other) {
            return (other != null)
                && Name.Equals(other.Name);
        }
    }
}
