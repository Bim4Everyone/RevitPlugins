using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.Navigator {
    internal class ClashViewModel : BaseViewModel {
        private string _firstName;
        private string _firstDocumentName;
        private string _secondName;
        private string _secondDocumentName;
        private string _firstLevel;
        private string _secondLevel;
        private readonly ClashModel _clash;

        public ClashViewModel(ClashModel clash) {
            var levelPropertyName = LabelUtils.GetLabelFor(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM);

            FirstName = clash.MainElement.Name;
            FirstDocumentName = clash.MainElement.Document.Title;
            FirstLevel = clash.MainElement.IsExistsParam(levelPropertyName) ? clash.MainElement.GetParam(levelPropertyName).AsValueString() : null;

            SecondName = clash.OtherElement.Name;
            SecondLevel = clash.OtherElement.IsExistsParam(levelPropertyName) ? clash.OtherElement.GetParam(levelPropertyName).AsValueString() : null;
            SecondDocumentName = clash.OtherElement.Document.Title;
            _clash = clash;
        }

        public string FirstName {
            get => _firstName;
            set => this.RaiseAndSetIfChanged(ref _firstName, value);
        }

        public string FirstDocumentName {
            get => _firstDocumentName;
            set => this.RaiseAndSetIfChanged(ref _firstDocumentName, value);
        }

        public string FirstLevel {
            get => _firstLevel;
            set => this.RaiseAndSetIfChanged(ref _firstLevel, value);
        }

        public string SecondName {
            get => _secondName;
            set => this.RaiseAndSetIfChanged(ref _secondName, value);
        }

        public string SecondLevel {
            get => _secondLevel;
            set => this.RaiseAndSetIfChanged(ref _secondLevel, value);
        }

        public string SecondDocumentName {
            get => _secondDocumentName;
            set => this.RaiseAndSetIfChanged(ref _secondDocumentName, value);
        }

        public ElementId GetElementId() {
            return _clash.MainElement.Id;
        }
    }
}
