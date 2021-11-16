using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectParams;
using dosymep.WPF.ViewModels;

using RevitRooms.Views;

namespace RevitRooms.ViewModels {
    internal class InfoElementViewModel : BaseViewModel {
        public string Message { get; set; }
        public string Description { get; set; }

        public TypeInfo TypeInfo { get; set; }
        public ObservableCollection<MessageElementViewModel> Elements { get; set; }
    }

    internal class MessageElementViewModel {
        public string Description { get; set; }
        public IElementViewModel<Element> Element { get; set; }

        public ElementId ElementId { get; }

        public string Name => Element.Name;
        public string PhaseName => Element.PhaseName;
        public string LevelName => Element.LevelName;
        public string CategoryName => Element.CategoryName;

        public ICommand ShowElementCommand => Element.ShowElementCommand;
        public ICommand SelectElementCommand => Element.SelectElementCommand;
    }

    internal class InfoElement {
        public static InfoElement RedundantRooms { get; } = new InfoElement() {
            TypeInfo = TypeInfo.Error,
            Message = "Избыточные или не окруженные помещения.",
            Description = "Для запуска плагина следует исправить или удалить все неокруженные и избыточные зоны."
        };

        public static InfoElement RedundantAreas { get; } = new InfoElement() {
            TypeInfo = TypeInfo.Error,
            Message = "Избыточные или не окруженные зоны.",
            Description = "Для запуска плагина следует исправить или удалить все неокруженные и избыточные помещения."
        };

        public static InfoElement RequiredParams { get; } = new InfoElement() {
            TypeInfo = TypeInfo.Error,
            Message = "Не заполнен обязательный параметр \"{0}\".",
            Description = "У данных помещений не заполнен ключевой параметр. Для запуска плагина необходимо заполнить значение этого параметра у каждого помещения в списке."
        };

        public static InfoElement NotEqualGroupType { get; } = new InfoElement() {
            TypeInfo = TypeInfo.Error,
            Message = "Не совпадают значения параметров групп \"{0} - {1}\"и типа групп.",
            Description = $"Для помещений  данной группы на указанных этажах не совпадает значение параметра \"{ProjectParamsConfig.Instance.RoomTypeGroupName.Name}\"."
                + Environment.NewLine
                + $"Для всех помещений в пределах одной группы и одного этажа параметры \"{ProjectParamsConfig.Instance.RoomTypeGroupName.Name}\" и \"{ProjectParamsConfig.Instance.RoomGroupName.Name}\" должны соответствовать друг другу."
        };
        
        public static InfoElement NotEqualSectionDoors { get; } = new InfoElement() {
            TypeInfo = TypeInfo.Warning,
            Message = $"Проверка параметра \"{ProjectParamsConfig.Instance.RoomSectionName.Name}\" по дверям",
            Description = $"Обнаружены помещения принадлежащие разным секциям, но имеющие доступ друг к другу через двери. Проверьте корректность параметра \"{ProjectParamsConfig.Instance.RoomSectionName.Name}\" для помещений, соединенных данными дверьми."
        };
        
        public static InfoElement CountourIntersectRooms { get; } = new InfoElement() {
            TypeInfo = TypeInfo.Warning,
            Message = "Найдены самопересечения помещений.",
            Description = "Для данных помещений обнаружены самопересечения контуров."
        };

        public static InfoElement BigChangesRoomAreas { get; } = new InfoElement() {
            TypeInfo = TypeInfo.Info,
            Message = "Большие изменения в площади помещений."
        };

        public static InfoElement BigChangesFlatAreas { get; } = new InfoElement() {
            TypeInfo = TypeInfo.Info,
            Message = "Большие изменения в площади квартир."
        };


        public static InfoElement BigChangesAreas { get; } = new InfoElement() {
            TypeInfo = TypeInfo.Info,
            Message = "Большие изменения в площади зон."
        };

        public string Message { get; set; }
        public string Description { get; set; }

        public TypeInfo TypeInfo { get; set; }

        public InfoElement FormatMessage(params string[] args) {
            return new InfoElement() {
                TypeInfo = TypeInfo,
                Description = Description,
                Message = string.Format(Message, args)
            };
        }
    }
}
