using dosymep.WPF.ViewModels;

namespace RevitRefreshLinks.ViewModels;
internal abstract class PathInfoViewModel : BaseViewModel {
    protected PathInfoViewModel() {

    }

    public abstract string Name { get; }

    public abstract string FullName { get; }

    public abstract bool IsDirectory { get; }

    public abstract long Length { get; }
}
