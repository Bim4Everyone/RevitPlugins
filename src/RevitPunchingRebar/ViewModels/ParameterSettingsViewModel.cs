using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPunchingRebar.Models;

namespace RevitPunchingRebar.ViewModels;

/// <summary>Черновик имён параметров. Проверки выполняются только при работе с каркасами.</summary>
internal sealed class ParameterSettingsViewModel : BaseViewModel {
    private readonly List<ParameterBindingViewModel> _parameters;

    public ParameterSettingsViewModel(FrameParameterSettings settings, ILocalizationService localization) {
        _parameters = FrameParameterCatalog.Definitions.Select(definition => new ParameterBindingViewModel(
            definition, settings.GetBinding(definition.Role), localization)).ToList();
        ParameterGroups = new ListCollectionView(_parameters);
        ParameterGroups.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ParameterBindingViewModel.Group)));
        ResetCommand = RelayCommand.Create(Reset);
    }

    public ICollectionView ParameterGroups { get; }
    public ICommand ResetCommand { get; }

    public FrameParameterSettings GetSettings() {
        FrameParameterSettings settings = new FrameParameterSettings();
        foreach(ParameterBindingViewModel parameter in _parameters) {
            settings.Bindings[parameter.Definition.Role.ToString()] = new FrameParameterBinding {
                TargetName = parameter.ParameterName?.Trim()
            };
        }

        return settings;
    }

    private void Reset() {
        foreach(ParameterBindingViewModel parameter in _parameters) {
            parameter.ResetCommand.Execute(null);
        }
    }
}
