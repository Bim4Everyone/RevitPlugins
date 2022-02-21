using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class ElementInfosViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private ViewOrientation3D _orientation;
        private ObservableCollection<ElementInfoViewModel> _elementIfos;
        private List<TypeInfoViewModel> _types;
        private CollectionViewSource _elementInfosViewSource;

        public ElementInfosViewModel(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
            TypeInfos = new List<TypeInfoViewModel>();
            ElementInfos = new ObservableCollection<ElementInfoViewModel>();
            _orientation = _revitRepository.GetOrientation3D();
            SelectElementCommand = new RelayCommand(SelectElement, p => true);
            InitializeTypeInfo();
            ElementInfosViewSource = new CollectionViewSource() { 
                Source = ElementInfos 
            };
            TypeInfoCheckedCommand = new RelayCommand(TypeInfoChecked, p => true);
            ElementInfosViewSource.Filter += ElementInfosViewSourceFilter;
        }

        public ICommand SelectElementCommand { get; set; }
        public ICommand TypeInfoCheckedCommand { get; set; }

        public ObservableCollection<ElementInfoViewModel> ElementInfos {
            get => _elementIfos;
            set => this.RaiseAndSetIfChanged(ref _elementIfos, value);
        }


        public List<TypeInfoViewModel> TypeInfos {
            get => _types;
            set => this.RaiseAndSetIfChanged(ref _types, value);
        }

        public CollectionViewSource ElementInfosViewSource { 
            get => _elementInfosViewSource; 
            set => this.RaiseAndSetIfChanged(ref _elementInfosViewSource, value); 
        }

        public void UpdateCollection() {
            ElementInfos = new ObservableCollection<ElementInfoViewModel>(ElementInfos.Distinct());
            ElementInfosViewSource.Source = ElementInfos;
        }

       

        private void SelectElement(object p) {
            if(p is ElementInfoViewModel elementInfo) {
                if(_revitRepository.GetElementById(elementInfo.ElementId) is Autodesk.Revit.DB.ElementType elementType) {
                    TaskDialog.Show("Revit", "Невозможно подобрать вид для отображения элемента.");
                } else {
                    _revitRepository.SelectAndShowElement(elementInfo.ElementId, _orientation);
                }
            }
        }

        private void InitializeTypeInfo() {
            foreach(var value in Enum.GetValues(typeof(TypeInfo))) {
                TypeInfos.Add(new TypeInfoViewModel() {
                    TypeInfo = (TypeInfo) value,
                    IsChecked = true
                });
            }
        }

        private void ElementInfosViewSourceFilter(object sender, FilterEventArgs e) {
            if (e.Item is ElementInfoViewModel elementInfo) {
                e.Accepted = TypeInfos
                    .Where(t => t.IsChecked)
                    .Any(t => t.TypeInfo == elementInfo.TypeInfo);

            }
        }

        private void TypeInfoChecked(object p) {
            ElementInfosViewSource.View.Refresh();
        }
    }

    internal class TypeInfoViewModel : BaseViewModel {
        private bool _isChecked;
        private TypeInfo _typeInfo;

        public bool IsChecked {
            get => _isChecked;
            set => this.RaiseAndSetIfChanged(ref _isChecked, value);
        }

        public TypeInfo TypeInfo { 
            get => _typeInfo; 
            set => this.RaiseAndSetIfChanged(ref _typeInfo, value); 
        }
    }
}