using System;
using System.Collections.Generic;
using System.ComponentModel;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models;
using RevitParamsChecker.Models.Rules;

namespace RevitParamsChecker.ViewModels.Rules;

internal class RuleViewModel : BaseViewModel, IEquatable<RuleViewModel>, IName {
    private readonly Rule _rule;
    private readonly ILocalizationService _localization;
    private readonly Guid _guid;
    private string _name;
    private string _description;
    private bool _modified;

    public RuleViewModel(
        Rule rule,
        ILocalizationService localization) {
        _rule = rule ?? throw new ArgumentNullException(nameof(rule));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _guid = Guid.NewGuid();
        Name = _rule.Name;
        Description = _rule.Description;
        RootSet = new ParamsSetViewModel(_rule.RootRule, _localization);

        PropertyChanged += OnModelPropertyChanged;
        RootSet.PropertyChanged += OnRootSetChanged;
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string Description {
        get => _description;
        set => RaiseAndSetIfChanged(ref _description, value);
    }

    public bool Modified {
        get => _modified;
        set => RaiseAndSetIfChanged(ref _modified, value);
    }

    public ParamsSetViewModel RootSet { get; }

    public bool Equals(RuleViewModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return _guid.Equals(other._guid);
    }

    public override bool Equals(object obj) {
        return Equals(obj as RuleViewModel);
    }

    public override int GetHashCode() {
        return _guid.GetHashCode();
    }

    public Rule GetRule() {
        _rule.Name = Name;
        _rule.Description = Description;
        _rule.RootRule = RootSet.GetRule();
        return _rule;
    }

    private void OnRootSetChanged(object sender, PropertyChangedEventArgs e) {
        Modified = true;
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(Description)
           || e.PropertyName == nameof(Name)) {
            Modified = true;
        }
    }
}
