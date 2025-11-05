using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using dosymep.WPF.ViewModels;

namespace RevitLoadFamilies.Models;
internal class FamilyConfig : BaseViewModel {
    private string _name;
    private List<string> _familyPaths;

    public FamilyConfig() {
        FamilyPaths = [];
    }
    public FamilyConfig(string name) : this() {
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
