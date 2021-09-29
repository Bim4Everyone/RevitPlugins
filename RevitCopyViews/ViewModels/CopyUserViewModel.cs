using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitCopyViews.ViewModels {
    internal class CopyUserViewModel : BaseViewModel {
        private string _prefix;
        private string _lastName;
        private string _errorText;

        public CopyUserViewModel() {
            CopyUserCommand = new RelayCommand(CopyUser, CanCopyUser);
        }

        public Document Document { get; set; }
        public UIDocument UIDocument { get; set; }
        public Application Application { get; set; }

        public List<View> Views { get; set; }
        public List<string> GroupViews { get; set; }
        public List<string> RestrictedViewNames { get; set; }

        public string Prefix {
            get => _prefix;
            set => this.RaiseAndSetIfChanged(ref _prefix, value);
        }

        public string LastName {
            get => _lastName;
            set => this.RaiseAndSetIfChanged(ref _lastName, value);
        }

        public string GroupView {
            get { return "01 " + LastName; }
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ICommand CopyUserCommand { get; }

        private void CopyUser(object p) {
            var projectParameters = ProjectParameters.Create(Application);
            projectParameters.SetupBrowserOrganization(Document);
            projectParameters.SetupRevitParams(Document, ProjectParamsConfig.Instance.ViewGroup, ProjectParamsConfig.Instance.ProjectStage);

            var createdViews = new List<ElementId>();
            using(var transaction = new Transaction(Document)) {
                transaction.Start("Копирование видов");

                View viewTemplate = CreateViewTemplate(GroupView);
                ParameterFilterElement paramFilter = CreateParameterFilterElement(GroupView);
                AddParamFilter(viewTemplate, paramFilter);

                foreach(View revitView in Views) {
                    View newView = (View) Document.GetElement(revitView.Duplicate(ViewDuplicateOption.Duplicate));

                    newView.Name = GetViewName(revitView);
                    if(HasViewTemplate(newView)) {
                        newView.ViewTemplateId = viewTemplate?.Id ?? ElementId.InvalidElementId;
                    } else {
                        newView.ViewTemplateId = ElementId.InvalidElementId;
                        newView.SetParamValue(ProjectParamsConfig.Instance.ViewGroup, GroupView);

                        if(newView.ViewType != ViewType.ThreeD) {
                            AddParamFilter(newView, paramFilter);
                        }
                    }

                    createdViews.Add(newView.Id);
                }

                transaction.Commit();
            }

            UIDocument.Selection.SetElementIds(createdViews);
        }

        private bool CanCopyUser(object p) {
            if(string.IsNullOrEmpty(LastName)) {
                ErrorText = "Не заполнена фамилия.";
                return false;
            }

            if(string.IsNullOrEmpty(Prefix)) {
                ErrorText = "Не заполнен префикс.";
                return false;
            }

            if(GroupViews.Any(item => item.Equals(GroupView, StringComparison.CurrentCultureIgnoreCase))) {
                ErrorText = "Данная группа видов уже используется.";
                return false;
            }

            if(RestrictedViewNames.Any(item => item.StartsWith(Prefix + "_", StringComparison.CurrentCultureIgnoreCase))) {
                ErrorText = "Данный префикс уже используется.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private string GetViewName(View revitView) {
            return revitView.Name.Replace("User_", Prefix + "_");
        }

        private View CreateViewTemplate(string viewTemplateName) {
            View viewTemplate = new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(View))
                .OfType<View>()
                .Where(item => item.IsTemplate)
                .FirstOrDefault(item => item.Name.Equals("99 User_План"));

            if(viewTemplate == null) {
                return null;
            }

            ElementId viewTemplateId = ElementTransformUtils.CopyElements(Document, new[] { viewTemplate.Id }, Document, Transform.Identity, new CopyPasteOptions()).First();

            viewTemplate = (View) Document.GetElement(viewTemplateId);
            viewTemplate.Name = viewTemplateName;
            viewTemplate.SetParamValue(ProjectParamsConfig.Instance.ViewGroup, GroupView);
            
            return viewTemplate;
        }

        private ParameterFilterElement CreateParameterFilterElement(string filterName) {
            ParameterElement parameterElement = new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(ParameterElement))
                .OfType<ParameterElement>()
                .FirstOrDefault(item => item.Name.Equals(ProjectParamsConfig.Instance.ViewGroup.Name));


            var logicalFilter = new LogicalAndFilter(new[] {
#if D2020 || R2020 || D2021 || R2021
            new ElementParameterFilter(new FilterInverseRule(new FilterStringRule(new ParameterValueProvider(parameterElement.Id), new FilterStringEquals(), filterName, true)))
#else
            new ElementParameterFilter(new FilterInverseRule(new FilterStringRule(new ParameterValueProvider(parameterElement.Id), new FilterStringEquals(), filterName)))
#endif
                
            });

            var categories = new[] {
                Category.GetCategory(Document, BuiltInCategory.OST_Elev).Id,
                Category.GetCategory(Document, BuiltInCategory.OST_Callouts).Id,
                Category.GetCategory(Document, BuiltInCategory.OST_Sections).Id,
            };

            return ParameterFilterElement.Create(Document, $"Виды_НЕ_{filterName}", categories, logicalFilter);
        }

        private void AddParamFilter(View view, ParameterFilterElement paramFilter) {
            if(view == null) {
                return;
            }

            foreach(var filter in view.GetFilters()) {
                view.RemoveFilter(filter);
            }

            view.AddFilter(paramFilter.Id);
            view.SetFilterVisibility(paramFilter.Id, false);
        }

        private bool HasViewTemplate(View view) {
            return view.ViewType == ViewType.FloorPlan
                || view.ViewType == ViewType.CeilingPlan
                || view.ViewType == ViewType.EngineeringPlan;
        }
    }
}
