using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPunchingRebar.Models;

namespace RevitPunchingRebar.ViewModels;

/// <summary>
/// Назначение параметра и редактируемое имя без обращения к элементам модели.
/// </summary>
internal sealed class ParameterBindingViewModel : BaseViewModel {
    private string _parameterName;

    public ParameterBindingViewModel(
        FrameParameterDefinition definition, FrameParameterBinding binding,
        ILocalizationService localization) {
        Definition = definition;
        string groupKey = GetGroupKey(definition);
        Group = localization.GetLocalizedString($"Parameters.Group.{groupKey}");
        Title = localization.GetLocalizedString(definition.TitleKey);
        Description = localization.GetLocalizedString(definition.DescriptionKey);
        DefaultHint = string.Format(localization.GetLocalizedString("Parameters.Default"), definition.DefaultName);
        ResetLabel = localization.GetLocalizedString("Parameters.Reset");
        ParameterName = binding.TargetName;
        ResetCommand = RelayCommand.Create(() => ParameterName = Definition.DefaultName);
    }

    public FrameParameterDefinition Definition { get; }
    public string Group { get; }
    public string Title { get; }
    public string Description { get; }
    public string DefaultHint { get; }
    public string ResetLabel { get; }
    public ICommand ResetCommand { get; }
    public string ParameterName {
        get => _parameterName;
        set => RaiseAndSetIfChanged(ref _parameterName, value);
    }

    private static string GetGroupKey(FrameParameterDefinition definition) {
        if(definition.IsCopy || definition.Role == FrameParameterRole.KrGroup) {
            return "Data";
        }

        if(definition.Role == FrameParameterRole.WidthStep || definition.Role == FrameParameterRole.HeightStep) {
            return "Geometry";
        }

        return definition.Role.ToString().StartsWith("Stirrup") ? "Stirrups" : "Longitudinal";
    }
}
