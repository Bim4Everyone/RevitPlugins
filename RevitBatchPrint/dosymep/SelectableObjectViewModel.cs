using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dosymep.WPF.ViewModels {
    internal class SelectableObjectViewModel<T> : BaseViewModel {
        private bool _isSelected;

        public SelectableObjectViewModel(T objectData) {
            ObjectData = objectData;
            DisplayData = ObjectData.ToString();
        }

        public SelectableObjectViewModel(T objectData, bool isSelected) {
            ObjectData = objectData;
            IsSelected = isSelected;

            DisplayData = ObjectData.ToString();
        }

        public T ObjectData { get; }
        public virtual string DisplayData { get; }

        public bool IsSelected {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }
    }
}