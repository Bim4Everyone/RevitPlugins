using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class ElementInfoViewModel : BaseViewModel {
        private string _message;
        private ElementId _elementId;
        private TypeInfo _typeInfo;
        private ElementType _elementType;

        public ElementInfoViewModel(ElementId elementId, InfoElement infoElement) {
            ElementId = elementId;
            Message = infoElement.Message;
            ElementType = infoElement.ElementType;
            TypeInfo = infoElement.TypeInfo;
        }

        public ElementInfoViewModel(ElementId elementId, InfoElement infoElement, params string[] args) {
            ElementId = elementId;
            infoElement.FormatMessage(args);
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

        public string Message {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public ElementId ElementId {
            get => _elementId;
            set => this.RaiseAndSetIfChanged(ref _elementId, value);
        }
    }

    internal enum TypeInfo {
        [Description("Error")]
        Error,
        [Description("Информация")]
        Info,
        [Description("Предупреждение")]
        Warning
    }

    internal enum ElementType {
        [Description("Перемычка")]
        Lintel,
        [Description("Проем")]
        Opening
    }
}
