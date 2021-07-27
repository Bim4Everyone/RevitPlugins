using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace Superfilter {
    internal class CategoryViewModel : BaseViewModel {
        private bool _selected;

        public CategoryViewModel(Category category, IEnumerable<Element> elements) {
            Category = category;
            Elements = new ObservableCollection<Element>(elements.OrderBy(item => item.Name));
        }

        public Category Category { get; }
        public ObservableCollection<Element> Elements { get; }
        
        public string Name {
            get => $"{Category?.Name ?? "Без категории"} [{Elements.Count}]";
        }

        public bool Selected {
            get => _selected;
            set {
                _selected = value;
                OnPropertyChanged(nameof(Selected));
            }
        }
    }
}
