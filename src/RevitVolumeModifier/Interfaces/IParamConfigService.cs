using System.Collections.ObjectModel;
using System.ComponentModel;

using RevitVolumeModifier.Models;

namespace RevitVolumeModifier.Interfaces;

internal interface IParamConfigService {
    event PropertyChangedEventHandler PropertyChanged;
    ObservableCollection<ParamModel> ParamModels { get; }
    bool HasWarnings { get; }
    void LoadConfig();
    void SaveConfig();
    void UpdateParamWarnings();
}
