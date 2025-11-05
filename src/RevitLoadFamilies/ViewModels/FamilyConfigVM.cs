using System.Collections.Generic;

using dosymep.WPF.ViewModels;

namespace RevitLoadFamilies.Models;
internal class FamilyConfigVM : BaseViewModel {
    private string _name;
    private List<string> _familyPaths;

    public FamilyConfigVM(string name) {
        Name = name;
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public List<string> FamilyPaths {
        get => _familyPaths;
        set => RaiseAndSetIfChanged(ref _familyPaths, value);
    }
}
