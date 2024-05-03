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
        
        private bool _skipObject;

        public ModelObjectViewModel(ModelObject modelObject) {
            _modelObject = modelObject;
        }

        public string Name => _modelObject.Name;
        public string FullName => _modelObject.FullName;
        public bool IsFolder => _modelObject.IsFolder;
        public bool HasChildren => _modelObject.HasChildren;

        public bool SkipObject {
            get => _skipObject;
            set => this.RaiseAndSetIfChanged(ref _skipObject, value);
        }

        public override string ToString() {
            return FullName;
        }
    }
}
