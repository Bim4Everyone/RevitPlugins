using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Autodesk.Revit.DB;

using DevExpress.Mvvm;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitApartmentPlans.Models;
using RevitApartmentPlans.Views;

namespace RevitApartmentPlans.ViewModels {
    internal class ViewTemplatesViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ViewTemplateAdditionViewModel _viewTemplateAdditionViewModel;

        public ViewTemplatesViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ViewTemplateAdditionViewModel viewTemplateAdditionViewModel,
            IDialogService dialogService) {

            _pluginConfig = pluginConfig
                ?? throw new ArgumentNullException(nameof(pluginConfig));
            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _viewTemplateAdditionViewModel = viewTemplateAdditionViewModel
                ?? throw new ArgumentNullException(nameof(viewTemplateAdditionViewModel));
            DialogService = dialogService
                ?? throw new ArgumentNullException(nameof(dialogService));

            AddViewTemplateCommand = RelayCommand.Create(AddViewTemplate);
            RemoveViewTemplateCommand
                = RelayCommand.Create<ViewTemplateViewModel>(RemoveViewTemplate, CanRemoveViewTemplate);
            ViewTemplates = new ObservableCollection<ViewTemplateViewModel>();
            LoadConfig();
        }


        public IDialogService DialogService { get; }

        public ICommand AddViewTemplateCommand { get; }

        public ICommand RemoveViewTemplateCommand { get; }

        public ObservableCollection<ViewTemplateViewModel> ViewTemplates { get; }

        private void AddViewTemplate() {
            _viewTemplateAdditionViewModel.SetAlreadyAddedViewTemplates(ViewTemplates);

            var result = DialogService.ShowDialog(MessageButton.OKCancel, "Добавление шаблона", nameof(AddViewTemplateView), _viewTemplateAdditionViewModel);
            if(result == MessageResult.OK) {

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
            ElementId[] templatesIds = _pluginConfig.GetSettings(_revitRepository.Document)?.ViewTemplates
                ?? Array.Empty<ElementId>();
            ICollection<ViewPlan> templates = _revitRepository.GetViewTemplates(templatesIds);
            foreach(ViewPlan template in templates) {
                ViewTemplates.Add(new ViewTemplateViewModel(template));
            }
        }
    }
}
