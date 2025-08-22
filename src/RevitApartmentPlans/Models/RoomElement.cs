using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitApartmentPlans.Models;
internal class RoomElement {
    private readonly RevitLinkInstance _link;

    /// <summary>
    /// Создает обертку помещения из связи
    /// </summary>
    /// <param name="room">Помещение из связи</param>
    /// <param name="link">Связь, в которой находится помещение</param>
    /// <exception cref="System.ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public RoomElement(Room room, RevitLinkInstance link) {
        Room = room ?? throw new System.ArgumentNullException(nameof(room));
        _link = link ?? throw new System.ArgumentNullException(nameof(link));
        Transform = link.GetTransform();
    }

    /// <summary>
    /// Создает обертку помещения из активного файла
    /// </summary>
    /// <param name="room">Помещение из активного файла</param>
    public RoomElement(Room room) {
        Room = room ?? throw new System.ArgumentNullException(nameof(room));
        Transform = Transform.Identity;
    }


    public Room Room { get; }

    /// <summary>
    /// Трансформация помещения относительно активного файла
    /// </summary>
    public Transform Transform { get; }

    public Reference GetReference() {
        return _link != null ? new Reference(Room).CreateLinkReference(_link) : new Reference(Room);
    }
}
