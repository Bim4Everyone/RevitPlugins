﻿using System;
using System.Collections.Generic;
using System.Linq;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitOpeningPlacement.Models.Configs {
    /// <summary>
    /// Класс для обертки настроек расстановки заданий на отверстия для элементов категории инженерных систем
    /// </summary>
    internal class MepCategory : IEquatable<MepCategory> {
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
        public bool IsSelected { get; set; }

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
        public List<Offset> Offsets { get; set; } = new List<Offset>();

        /// <summary>
        /// Категории конструкций (стены, перекрытия и т.п.), с которыми нужно выполнять проверку на пересечения и расстановку отверстий
        /// </summary>
        public List<StructureCategory> Intersections { get; set; } = new List<StructureCategory>();

        /// <summary>
        /// Значение округления габаритов итогового размещенного задания на отверстие в месте пересечения элемента данной категории и конструктивного элемента
        /// </summary>
        public int Rounding { get; set; }

        /// <summary>
        /// Правила фильтрации элементов данной категории
        /// </summary>
        public Set Set { get; set; } = new Set();


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
