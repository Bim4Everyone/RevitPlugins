using dosymep.WPF.ViewModels;

namespace RevitRefreshLinks.ViewModels {
    internal abstract class PathInfoViewModel : BaseViewModel {
        protected PathInfoViewModel() {

        }

        public abstract string Name { get; }

        public abstract bool IsDirectory { get; }
    }
}
