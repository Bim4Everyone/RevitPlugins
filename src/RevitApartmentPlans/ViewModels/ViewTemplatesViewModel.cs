using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.ViewModels {
    internal class ViewTemplatesViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        public ViewTemplatesViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig ?? throw new System.ArgumentNullException(nameof(pluginConfig));
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));

            AddViewTemplateCommand = RelayCommand.Create(AddViewTemplate);
            RemoveViewTemplateCommand
                = RelayCommand.Create<ViewTemplateViewModel>(RemoveViewTemplate, CanRemoveViewTemplate);
            ViewTemplates = new ObservableCollection<ViewTemplateViewModel>();
            LoadConfig();
        }


        public ICommand AddViewTemplateCommand { get; }

        public ICommand RemoveViewTemplateCommand { get; }

        public ObservableCollection<ViewTemplateViewModel> ViewTemplates { get; }


        private void AddViewTemplate() {
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
            ElementId[] templatesIds = _pluginConfig.GetSettings(_revitRepository.Document).ViewTemplates;
            ICollection<ViewPlan> templates = _revitRepository.GetViewPlans(templatesIds);
            foreach(ViewPlan template in templates) {
                ViewTemplates.Add(new ViewTemplateViewModel(template));
            }
        }
    }
}
