using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitApartmentPlans.Models;
internal class Apartment {
    private readonly ICollection<RoomElement> _rooms;

    /// <summary>
    /// Конструктор квартиры из активного документа на основе коллекции помещений. 
    /// Помещения могут быть как в активном документе, так и в связанном файле.
    /// </summary>
    /// <param name="rooms">Коллекция помещений квартиры, в которой находися как минимум 1 элемент.</param>
    /// <param name="name">Название квартиры.</param>
    /// <param name="level">Уровень квартиры из активного документа.</param>
    /// <exception cref="ArgumentNullException">Исключение, если один из обязательных параметров null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Исключение, если коллекция помещений пустая</exception>
    public Apartment(ICollection<RoomElement> rooms, string name, Level level) {
        if(rooms is null) {
            throw new ArgumentNullException(nameof(rooms));
        }
        if(string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentNullException(nameof(name));
        }
        if(rooms.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(rooms));
        }

        _rooms = rooms;
        Name = name;
        LevelName = level.Name;
        LevelId = level.Id;
    }


    /// <summary>
    /// Номер квартиры
    /// </summary>
    public string Name { get; }

    public ElementId LevelId { get; }

    public string LevelName { get; }


    public IReadOnlyCollection<RoomElement> GetRooms() {
        return new ReadOnlyCollection<RoomElement>(_rooms.ToArray());
    }
}
