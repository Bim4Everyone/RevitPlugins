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
            ProjectParameters.Create(Application).SetupBrowserOrganization(Document);

            var createdViews = new List<ElementId>();
            using(var transaction = new Transaction(Document)) {
                transaction.Start("Копирование видов");

                foreach(View revitView in Views) {
                    View newView = (View) Document.GetElement(revitView.Duplicate(ViewDuplicateOption.Duplicate));
                    newView.Name = GetViewName(revitView);

                    // У некоторых видов установлен шаблон,
                    // у которого заблокировано редактирование атрибута ProjectParamsConfig.Instance.ViewGroup
                    // удаление шаблона разрешает изменение данного атрибута
                    newView.ViewTemplateId = ElementId.InvalidElementId;
                    newView.SetParamValue(ProjectParamsConfig.Instance.ViewGroup, GroupView);

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
    }
}
