using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitApartmentPlans.Models {
    internal class Apartment {
        private readonly ICollection<Room> _rooms;

        /// <summary>
        /// Конструктор квартиры на основе коллекции помещений.
        /// </summary>
        /// <param name="rooms">Коллекция помещений квартиры, в которой находися как минимум 1 элемент.
        /// Также все помещения квартиры должны быть на одном и том же уровне.</param>
        /// <param name="name">Название квартиры.</param>
        /// <exception cref="ArgumentNullException">Исключение, если один из обязательных параметров null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Исключение, если коллекция помещений пустая</exception>
        /// <exception cref="ArgumentException">Исключение, если помещения расположены на разных уровнях</exception>
        public Apartment(ICollection<Room> rooms, string name) {
            if(rooms is null) {
                throw new ArgumentNullException(nameof(rooms));
            }
            if(string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentNullException(nameof(name));
            }
            if(rooms.Count == 0) {
                throw new ArgumentOutOfRangeException(nameof(rooms));
            }
            if(rooms.Select(r => r.LevelId).Distinct().Count() != 1) {
                throw new ArgumentException($"Помещения квартиры {name} расположены на разных уровнях.");
            }

            _rooms = rooms;
            Name = name;
            Level level = rooms.First().Level;
            LevelName = level.Name;
            LevelId = level.Id;
        }


        /// <summary>
        /// Номер квартиры
        /// </summary>
        public string Name { get; }

        public ElementId LevelId { get; }

        public string LevelName { get; }


        public IReadOnlyCollection<Room> GetRooms() {
            return new ReadOnlyCollection<Room>(_rooms.ToArray());
        }
    }
}
