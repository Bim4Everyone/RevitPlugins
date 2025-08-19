using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCopyViews.Models;

namespace RevitCopyViews.ViewModels;

internal class CopyUserViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _prefix;
    private string _lastName;
    private string _errorText;

    public CopyUserViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public List<View> Views { get; set; }
    public List<string> GroupViews { get; set; }
    public List<string> RestrictedViewNames { get; set; }

    public string Prefix {
        get => _prefix;
        set => RaiseAndSetIfChanged(ref _prefix, value);
    }

    public string LastName {
        get => _lastName;
        set => RaiseAndSetIfChanged(ref _lastName, value);
    }

    public string GroupView => "01 " + LastName;

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void LoadView() {
        _revitRepository.TransferStandards();
        
        List<View> views = _revitRepository.GetViews().ToList();
        Views = _revitRepository.GetUserViews(views).ToList();
        GroupViews = _revitRepository.GetGroupViews(views).ToList();
        RestrictedViewNames = _revitRepository.GetViewNames(views).ToList();
    }

    private void AcceptView() {
        using var transaction =
            _revitRepository.StartTransaction(_localizationService.GetLocalizedString("CopyUser.TransactionName"));

        foreach(var view in Views) {
            var newView = _revitRepository.GetElement<View>(view.Duplicate(ViewDuplicateOption.Duplicate));

            newView.Name = GetViewName(view);
            newView.ViewTemplateId = ElementId.InvalidElementId;
            newView.SetParamValue(ProjectParamsConfig.Instance.ViewGroup, GroupView);
        }

        transaction.Commit();
    }

    private bool CanAcceptView() {
        if(string.IsNullOrEmpty(LastName)) {
            ErrorText = _localizationService.GetLocalizedString("CopyUser.EmptyLastName");
            return false;
        }

        if(string.IsNullOrEmpty(Prefix)) {
            ErrorText = _localizationService.GetLocalizedString("CopyUser.EmptyPrefix");
            return false;
        }

        if(GroupViews.Any(item => item.Equals(GroupView, StringComparison.CurrentCultureIgnoreCase))) {
            ErrorText = _localizationService.GetLocalizedString("CopyUser.GroupExists", GroupView);
            return false;
        }

        if(RestrictedViewNames.Any(item =>
               item.StartsWith(Prefix + "_", StringComparison.CurrentCultureIgnoreCase))) {
            ErrorText = _localizationService.GetLocalizedString("CopyUser.PrefixExists", Prefix);
            return false;
        }

        ErrorText = null;
        return true;
    }

    private string GetViewName(View view) {
        return view.Name.Replace(RevitRepository.UserViewPrefix, Prefix + "_");
    }
}
