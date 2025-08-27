using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Ninject;
using Ninject.Syntax;

using RevitApartmentPlans.Models;
using RevitApartmentPlans.Views;

namespace RevitApartmentPlans.ViewModels;
internal class ViewTemplatesViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ViewTemplateAdditionViewModel _viewTemplateAdditionViewModel;
    private readonly IResolutionRoot _container;
    private readonly ILocalizationService _localization;

    public ViewTemplatesViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ViewTemplateAdditionViewModel viewTemplateAdditionViewModel,
        IResolutionRoot container,
        ILocalizationService localization) {

        _pluginConfig = pluginConfig
            ?? throw new ArgumentNullException(nameof(pluginConfig));
        _revitRepository = revitRepository
            ?? throw new ArgumentNullException(nameof(revitRepository));
        _viewTemplateAdditionViewModel = viewTemplateAdditionViewModel
            ?? throw new ArgumentNullException(nameof(viewTemplateAdditionViewModel));
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _localization = localization
            ?? throw new ArgumentNullException(nameof(localization));
        AddViewTemplateCommand = RelayCommand.Create(AddViewTemplate);
        RemoveViewTemplateCommand
            = RelayCommand.Create<ViewTemplateViewModel>(RemoveViewTemplate, CanRemoveViewTemplate);
        ViewTemplates = [];
        LoadConfig();
    }


    public ICommand AddViewTemplateCommand { get; }

    public ICommand RemoveViewTemplateCommand { get; }

    public ObservableCollection<ViewTemplateViewModel> ViewTemplates { get; }

    private void AddViewTemplate() {
        _viewTemplateAdditionViewModel.SetAlreadyAddedViewTemplates(ViewTemplates);
        var window = _container.Get<ViewTemplateAdditionWindow>();
        bool success = window.ShowDialog() ?? false;
        if(success && _viewTemplateAdditionViewModel.SelectedViewTemplate != null) {
            ViewTemplates.Add(_viewTemplateAdditionViewModel.SelectedViewTemplate);
        }
    }

    private void RemoveViewTemplate(ViewTemplateViewModel viewTemplateView) {
        ViewTemplates.Remove(viewTemplateView);
    }

    private bool CanRemoveViewTemplate(ViewTemplateViewModel viewTemplateView) {
        return viewTemplateView != null;
    }

    private void LoadConfig() {
        if(_pluginConfig == null) { throw new ArgumentNullException(nameof(_pluginConfig)); }
        if(ViewTemplates == null) { throw new ArgumentNullException(nameof(ViewTemplates)); }
        if(_revitRepository == null) { throw new ArgumentNullException(nameof(_revitRepository)); }

        ViewTemplates.Clear();
        var templatesIds = _pluginConfig.GetSettings(_revitRepository.Document)?.ViewTemplates
            ?? Array.Empty<ElementId>();
        var templates = _revitRepository.GetViewTemplates(templatesIds);
        foreach(var template in templates) {
            ViewTemplates.Add(new ViewTemplateViewModel(template, _localization));
        }
    }
}
