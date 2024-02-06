using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

namespace RevitMarkPlacement.ViewModels {
    internal class InfoElementViewModel : BaseViewModel, IEquatable<InfoElementViewModel> {
        private string _message;

        public InfoElementViewModel(InfoElement infoElement) {
            Message = infoElement.Message;
            ElementType = infoElement.ElementType;
            TypeInfo = infoElement.TypeInfo;
        }

        public InfoElementViewModel(InfoElement infoElement, params string[] args) {
            infoElement = infoElement.FormatMessage(args);
            Message = infoElement.Message;
            ElementType = infoElement.ElementType;
            TypeInfo = infoElement.TypeInfo;
        }

        public TypeInfo TypeInfo { get; }
        public ElementType ElementType { get; }
        public string Message { 
            get => _message; 
            set => this.RaiseAndSetIfChanged(ref _message, value); 
        }

        public override bool Equals(object obj) {
            return Equals(obj as InfoElementViewModel);
        }

        public bool Equals(InfoElementViewModel other) {
            return other != null &&
                   _message == other._message &&
                   TypeInfo == other.TypeInfo &&
                   ElementType == other.ElementType &&
                   Message == other.Message;
        }

        public override int GetHashCode() {
            int hashCode = -1339174723;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_message);
            hashCode = hashCode * -1521134295 + TypeInfo.GetHashCode();
            hashCode = hashCode * -1521134295 + ElementType.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Message);
            return hashCode;
        }
    }

    internal enum TypeInfo {
        [Description("Ошибки")]
        Error,
        [Description("Предупреждения")]
        Warning,
        [Description("Сообщения")]
        Info
    }

    internal enum ElementType {
        [Description("Типовая аннотация")]
        Annotation,
        [Description("Высотная отметка")]
        SpotDimension
    }
}
