using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using DevExpress.Mvvm.DataAnnotations;

using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class ElementInfoViewModel : BaseViewModel, IEquatable<ElementInfoViewModel> {
        private string _message;
        private ElementId _elementId;
        private TypeInfo _typeInfo;
        private ElementType _elementType;
        private string _levelName;
        private string _name;

        public ElementInfoViewModel(ElementId elementId, InfoElement infoElement) {
            ElementId = elementId;
            Message = infoElement.Message;
            ElementType = infoElement.ElementType;
            TypeInfo = infoElement.TypeInfo;
        }

        public ElementInfoViewModel(ElementId elementId, InfoElement infoElement, params string[] args) {
            ElementId = elementId;
            Message = infoElement.Message;
            ElementType = infoElement.ElementType;
            TypeInfo = infoElement.TypeInfo;
        }

        public TypeInfo TypeInfo {
            get => _typeInfo;
            set => this.RaiseAndSetIfChanged(ref _typeInfo, value);
        }

        public ElementType ElementType {
            get => _elementType;
            set => this.RaiseAndSetIfChanged(ref _elementType, value);
        }

        public bool IsCorrectValue => ElementId != ElementId.InvalidElementId;

        public string Message {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public ElementId ElementId {
            get => _elementId;
            set => this.RaiseAndSetIfChanged(ref _elementId, value);
        }

        public string LevelName {
            get => _levelName;
            set => this.RaiseAndSetIfChanged(ref _levelName, value);
        }

        public string Name { 
            get => _name; 
            set => this.RaiseAndSetIfChanged(ref _name, value); 
        }

        public override bool Equals(object obj) {
            return Equals(obj as ElementInfoViewModel);
        }

        public bool Equals(ElementInfoViewModel other) {
            return other != null &&
                   TypeInfo == other.TypeInfo &&
                   ElementType == other.ElementType &&
                   Message == other.Message &&
                   EqualityComparer<ElementId>.Default.Equals(ElementId, other.ElementId);
        }

        public override int GetHashCode() {
            int hashCode = 897683154;
            hashCode = hashCode * -1521134295 + TypeInfo.GetHashCode();
            hashCode = hashCode * -1521134295 + ElementType.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Message);
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(ElementId);
            return hashCode;
        }
    }

    internal enum TypeInfo {
        [Display(Name = "Ошибки")]
        Error,
        [Display(Name = "Предупреждения")]
        Warning,
        [Display(Name ="Сообщения")]
        Info
    }

    internal enum ElementType {
        [Description("Перемычка")]
        Lintel,
        [Description("Проем")]
        Opening,
        [Description("Настройки")]
        Config,
        [Description("Вид")]
        View
    }
}
