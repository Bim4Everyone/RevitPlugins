using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitServerFolders.Models;

namespace RevitServerFolders.ViewModels {
    internal sealed class ModelObjectViewModel : BaseViewModel {
        private readonly ModelObject _modelObject;
        private bool _isExpanded;

        public ModelObjectViewModel(ModelObject modelObject) {
            _modelObject = modelObject;
            Children = new ObservableCollection<ModelObjectViewModel>();
            LoadChildrenCommand = RelayCommand.CreateAsync(LoadChildren);
        }

        public ICommand LoadChildrenCommand { get; }

        public string Name => _modelObject.Name;
        public string FullName => _modelObject.FullName;
        public bool IsFolder => _modelObject.IsFolder;
        public bool HasChildren => _modelObject.HasChildren;

        public bool IsExpanded {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        public ObservableCollection<ModelObjectViewModel> Children { get; }

        public async Task LoadChildren() {
            Children.Clear();
            if(HasChildren) {
                foreach(ModelObject childrenObject in await _modelObject.GetChildrenObjects()) {
                    Children.Add(new ModelObjectViewModel(childrenObject));
                }

                IsExpanded = true;
            }
        }
    }
}
