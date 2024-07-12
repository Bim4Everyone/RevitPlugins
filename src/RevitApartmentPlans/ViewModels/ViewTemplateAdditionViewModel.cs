using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.ViewModels {
    /// <summary>
    /// Модель представления для добавления шаблона вида в перечень для создания планов
    /// </summary>
    internal class ViewTemplateAdditionViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        /// <summary>
        /// Все шаблоны видов в активном документе
        /// </summary>
        private readonly HashSet<ViewTemplateViewModel> _allViewTemplatesFromDoc;
        /// <summary>
        /// Уже добавленные шаблоны видов
        /// </summary>
        private readonly HashSet<ViewTemplateViewModel> _addedViewTemplates;

        public ViewTemplateAdditionViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));

            _allViewTemplatesFromDoc = new HashSet<ViewTemplateViewModel>(
                _revitRepository.GetViewTemplates()
                .Select(t => new ViewTemplateViewModel(t)));
            _addedViewTemplates = new HashSet<ViewTemplateViewModel>();
            EnabledViewTemplates = new ObservableCollection<ViewTemplateViewModel>();
            EnabledViewTypes = new ObservableCollection<ViewType>(_revitRepository.GetAllUsedViewTypes());
            SelectedViewType = EnabledViewTypes.First();
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }


        public ICommand AcceptViewCommand { get; }

        private string _errorText;
        public string ErrorText {
            get => _errorText;
            set => RaiseAndSetIfChanged(ref _errorText, value);
        }

        /// <summary>
        /// Типы шаблонов видов, которые можно добавить: план этажа/потолка
        /// </summary>
        public ObservableCollection<ViewType> EnabledViewTypes { get; }

        private ViewType _selectedViewType;
        /// <summary>
        /// Выбранный тип шаблона вида для добавления: план этажа/потолка
        /// </summary>
        public ViewType SelectedViewType {
            get => _selectedViewType;
            set {
                RaiseAndSetIfChanged(ref _selectedViewType, value);
                SelectedViewTemplate = null;
                UpdateEnabledViewTemplates();
            }
        }

        /// <summary>
        /// Шаблоны видов, которые можно добавить для создания планов по ним
        /// </summary>
        public ObservableCollection<ViewTemplateViewModel> EnabledViewTemplates { get; }

        private ViewTemplateViewModel _selectedViewTemplate;
        /// <summary>
        /// Выбранный шаблон вида, который нужно добавить
        /// </summary>
        public ViewTemplateViewModel SelectedViewTemplate {
            get => _selectedViewTemplate;
            set => RaiseAndSetIfChanged(ref _selectedViewTemplate, value);
        }


        /// <summary>
        /// Назначает уже добавленные шаблоны видов
        /// </summary>
        public void SetAlreadyAddedViewTemplates(ICollection<ViewTemplateViewModel> addedViewTemplates) {
            if(addedViewTemplates is null) {
                throw new ArgumentNullException(nameof(addedViewTemplates));
            }

            _addedViewTemplates.Clear();
            foreach(var template in addedViewTemplates) {
                _addedViewTemplates.Add(template);
            }
            UpdateEnabledViewTemplates();
        }

        private void AcceptView() {
            // nothing
        }

        private bool CanAcceptView() {
            if(SelectedViewTemplate is null) {
                ErrorText = "Выберите шаблон";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void UpdateEnabledViewTemplates() {
            EnabledViewTemplates.Clear();
            var viewTemplatesToAdd = _allViewTemplatesFromDoc
                .Where(t => t.ViewTemplateType == _selectedViewType)
                .Except(_addedViewTemplates)
                .OrderBy(t => t.Name);
            foreach(var viewTemplate in viewTemplatesToAdd) {
                EnabledViewTemplates.Add(viewTemplate);
            }
        }
    }
}
